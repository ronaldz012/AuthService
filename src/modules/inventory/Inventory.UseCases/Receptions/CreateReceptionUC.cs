using Auth.Contracts.Interfaces;
using Common.Data;
using Inventory.Contracts.Dtos.Receptions;
using Inventory.Data.Entities.Inventory;
using Inventory.Data.Entities.Products;
using Inventory.Data.Entities.Receptions;
using Inventory.Infrastructure.CodeGenerator;
using Inventory.UseCases.Products;
using Microsoft.EntityFrameworkCore;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Receptions;

public class CreateReceptionUc(
    InvDbContext context,
    ProductUseCases productUseCases,
    ICurrentUser currentUser,
    ITenantContext tenantContext)
{
    public async Task<Result<StockReceptionResultDto>> Execute(CreateStockReceptionDto dto)
    {
        var userId = currentUser.UserId;
        var schema = tenantContext.Schema;

        // -- 1. Ids de productos existentes referenciados ----------------------
        var productIds = dto.Items
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .ToList();

        var existingProductsData = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.InternalCode })
            .ToDictionaryAsync(x => x.Id, x => x.InternalCode);

        var idsOk = await productUseCases.ValidateProducts.Execute(productIds);
        if (!idsOk.IsSuccess) return new Error(idsOk.Error.Code, idsOk.Error.Message);

        // -- 2. Variantes existentes -------------------------------------------
        var productVariants = await GetProductVariants(dto);
        if (!productVariants.IsSuccess) return productVariants.Error;

        // -- 3. Marcas involucradas en productos NUEVOS ------------------------
        var newProductItems = dto.Items.Where(x => !x.ProductId.HasValue).ToList();
        var brandIds = newProductItems
            .Select(x => x.NewProduct!.BrandId)
            .Distinct()
            .ToList();

        // Traemos prefix y counter actual — sin depender de BrandUseCases
        var brands = await context.Brands
            .Where(b => brandIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b);

        // -- 4. Colores de todas las variantes nuevas (existentes y nuevos productos) --
        var allNewVariantColorIds = dto.Items
            .SelectMany(i => i.Variants)
            .Where(v => v.NewVariant != null)
            .Select(v => v.NewVariant!.ColorId)
            .Distinct()
            .ToList();

        var colors = await context.Colors
            .Where(c => allNewVariantColorIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Code);

        // -- 5. Transacción ----------------------------------------------------
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // -- 6. Reservar contadores por marca (atómico) --------------------
            //    Hacemos un UPDATE por marca que incrementa N lugares de golpe
            //    y retorna el nuevo valor. Así dos requests concurrentes nunca
            //    obtienen el mismo rango.
            var reservedStart = await ReserveProductCounters(newProductItems, brands,schema);
            if (!reservedStart.IsSuccess) return reservedStart.Error;

            // -- 6. Construir el grafo -----------------------------------------
            var newReception = new StockReception
            {
                BranchId = currentUser.BranchIds[0],
                Notes = dto.Notes,
                ReceivedAt = DateTime.UtcNow
            };
            var stockMovements = new List<StockMovement>();

            // — Productos EXISTENTES —
            var existingItems = dto.Items.Where(x => x.ProductId.HasValue).ToList();
            foreach (var item in existingItems)
            {
                var parentInternalCode = existingProductsData[item.ProductId!.Value];

                // Variantes existentes
                foreach (var variantDto in item.Variants.Where(v => v.ProductVariantId.HasValue))
                {
                    var productVariant = productVariants.Value
                        .First(x => x.Id == variantDto.ProductVariantId!.Value);

                    newReception.AddExistingVariant(
                        productVariant.Id,
                        variantDto.QuantityReceived,
                        variantDto.UnitCost);

                    productVariant.AddQuantity(variantDto.QuantityReceived, currentUser.BranchIds[0]);
                    stockMovements.Add(StockMovement.CreateReception(
                        currentUser.BranchIds[0], productVariant.Id, userId, variantDto.QuantityReceived));
                }

                // Variantes NUEVAS de producto existente
                foreach (var variantDto in item.Variants.Where(v => v.NewVariant != null))
                {
                    var colorCode = colors[variantDto.NewVariant!.ColorId];
                    var newPv = new ProductVariant
                    {
                        ProductId = item.ProductId!.Value,
                        Description = variantDto.NewVariant!.Description,
                        Size = variantDto.NewVariant.Size,
                        ColorId = variantDto.NewVariant.ColorId,
                        Price = variantDto.NewVariant.Price,
                        Sku = CodeGenerator.GenerateVariantSku(parentInternalCode, colorCode, variantDto.NewVariant.Size)
                    };

                    newPv.AddQuantity(variantDto.QuantityReceived, currentUser.BranchIds[0]);
                    newReception.Items.Add(new StockReceptionItem
                    {
                        ProductVariant = newPv,
                        QuantityReceived = variantDto.QuantityReceived,
                        UnitCost = variantDto.UnitCost
                    });
                    stockMovements.Add(StockMovement.CreateReceptionForNewVariant(
                        currentUser.BranchIds[0], newPv, userId, variantDto.QuantityReceived, string.Empty));
                }
            }

            // — Productos NUEVOS —
            // Llevamos un cursor por marca para asignar números del rango reservado
            var brandCursor = reservedStart.Value.ToDictionary(k => k.Key, v => v.Value);

            foreach (var item in newProductItems)
            {
                var brandId = item.NewProduct!.BrandId;
                var brand = brands[brandId];

                // Siguiente número del rango reservado para esta marca
                var myNumber = brandCursor[brandId];
                brandCursor[brandId]++;

                var internalCode = $"{brand.Prefix}-{myNumber}";

                var newProduct = new Product
                {
                    Name = item.NewProduct.Name,
                    Description = item.NewProduct.Description,
                    CategoryId = item.NewProduct.CategoryId,
                    BrandId = brandId,
                    BasePrice = item.NewProduct.BasePrice,
                    Gender = item.NewProduct.Gender,
                    InternalCode = internalCode
                };

                foreach (var variantDto in item.Variants)
                {
                    var colorCode = colors[variantDto.NewVariant!.ColorId];
                    var newVariant = new ProductVariant
                    {
                        Description = variantDto.NewVariant!.Description,
                        Price = variantDto.NewVariant.Price,
                        Size = variantDto.NewVariant.Size,
                        ColorId = variantDto.NewVariant.ColorId,
                        Sku = CodeGenerator.GenerateVariantSku(internalCode, colorCode, variantDto.NewVariant.Size)
                    };

                    newVariant.AddQuantity(variantDto.QuantityReceived, currentUser.BranchIds[0]);
                    newProduct.ProductVariants.Add(newVariant);
                    newReception.Items.Add(new StockReceptionItem
                    {
                        ProductVariant = newVariant,
                        QuantityReceived = variantDto.QuantityReceived,
                        UnitCost = variantDto.UnitCost
                    });
                    stockMovements.Add(StockMovement.CreateReceptionForNewVariant(
                        currentUser.BranchIds[0], newVariant, userId, variantDto.QuantityReceived));
                }

                context.Products.Add(newProduct);
            }

            context.StockReceptions.Add(newReception);
            context.StockMovements.AddRange(stockMovements);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            // -- 7. Retornar resultado -----------------------------------------
            var result = await context.StockReceptions
                .Where(r => r.Id == newReception.Id)
                .Select(r => new StockReceptionResultDto
                {
                    Id = r.Id,
                    BranchId = r.BranchId,
                    ReceivedAt = r.ReceivedAt,
                    Notes = r.Notes,
                    Items = r.Items.Select(i => new StockReceptionItemResultDto
                    {
                        ProductVariantId = i.ProductVariantId,
                        ProductName = i.ProductVariant.Product.Name,
                        VariantDescription = i.ProductVariant.Description,
                        QuantityReceived = i.QuantityReceived,
                        UnitCost = i.UnitCost
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
                throw new Exception(
                    $"La recepción {newReception.Id} se guardó pero no pudo ser consultada.");

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // -- Reserva N slots del counter por marca en un solo UPDATE atómico -------
    //    Retorna el primer número disponible para cada marca.
    //
    //    Ejemplo: había counter=5, se necesitan 3 productos de marca STR
    //    → UPDATE SET counter = 8 RETURNING 8
    //    → números disponibles: 6, 7, 8 (retornamos 6 como inicio del rango)
    private async Task<Result<Dictionary<Guid, int>>> ReserveProductCounters(
        List<CreateStockReceptionItemDto> newItems,
        Dictionary<Guid, Brand> brands,string schema)
    {
        if (newItems.Count == 0) return new Dictionary<Guid, int>();

        var countPerBrand = newItems
            .GroupBy(x => x.NewProduct!.BrandId)
            .ToDictionary(g => g.Key, g => g.Count());

        var reservedStart = new Dictionary<Guid, int>();
        var invalidBrandIds = countPerBrand.Keys.Except(brands.Keys).ToList();
        if (invalidBrandIds.Count != 0)
            return new Error("INVALID_BRAND", 
                $"Marcas no encontradas: {string.Join(", ", invalidBrandIds)}");

        foreach (var (brandId, count) in countPerBrand)
        {
            // UPDATE atómico: incrementa el counter y retorna el nuevo valor.
            // Dos requests concurrentes nunca obtienen el mismo rango porque
            // PostgresSQL serializa los Updates sobre la misma fila.
            var sql = "UPDATE \"" + schema + "\".\"Brands\" SET \"ProductCounter\" = \"ProductCounter\" + {0} WHERE \"Id\" = {1} RETURNING \"ProductCounter\"";

            var result = await context.Database
                .SqlQueryRaw<int>(sql, count, brandId)
                .ToListAsync();

            var newCounter = result[0];

            // Si counter era 5 y reservamos 3 → newCounter = 8
            // El rango reservado es [6, 7, 8], el inicio es 6
            reservedStart[brandId] = newCounter - count + 1;
        }

        return reservedStart;
    }

    private async Task<Result<List<ProductVariant>>> GetProductVariants(
        CreateStockReceptionDto dto)
    {
        var variantIds = dto.Items
            .SelectMany(pv => pv.Variants)
            .Where(v => v.ProductVariantId.HasValue)
            .Select(x => x.ProductVariantId!.Value)
            .ToList();

        if (variantIds.Count == 0) return new List<ProductVariant>();

        var productVariants = await context.ProductVariants
            .Include(x => x.BranchInventories)
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync();

        var missingIds = variantIds.Except(productVariants.Select(x => x.Id)).ToList();
        if (missingIds.Count != 0)
            return new Error("NOT_FOUND",
                $"VariantIds no encontrados: {string.Join(", ", missingIds)}");

        return productVariants;
    }
}