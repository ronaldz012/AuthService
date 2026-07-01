using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Seeder;

public class FeatureSeeder(IAuthDbContext context) : IDataSeeder
{
    public int Order => 2;

    public async Task SeedAsync()
    {
        if (await context.Features.AnyAsync()) return;

        // Lista genérica de acciones base para reutilizar en los módulos estándar
        List<FeatureAction> GetGenericCrudActions(string featureName) =>
        [
            new() { Key = "read", DisplayName = "Ver", Description = $"Permite listar y ver el detalle de {featureName}." },
            new() { Key = "create", DisplayName = "Crear", Description = $"Permite registrar nuevos {featureName}." },
            new() { Key = "update", DisplayName = "Editar", Description = $"Permite modificar {featureName} existentes." },
            new() { Key = "delete", DisplayName = "Eliminar", Description = $"Permite dar de baja o eliminar {featureName} del sistema." }
        ];

        var features = new List<Feature>
        {
            // ── Inventory ──────────────────────────────────────
            new()
            {
                Key         = "products",
                DisplayName = "Productos",
                Route       = "/products",
                Description = "Manage products catalog",
                Icon        = "box",
                Module      = Domain.Module.Inventory,
                IsMenu = true,
                CreatedAt   = DateTime.UtcNow,
                AvailableActions = GetGenericCrudActions("productos")
            },
            new()
            {
                Key         = "transfers",
                DisplayName = "Transferencias",
                Route       = "/transfers",
                Description = "Manage stock transfers between locations",
                Icon        = "transfer",
                Module      = Domain.Module.Inventory,
                IsMenu = true,
                CreatedAt   = DateTime.UtcNow,
                AvailableActions = GetGenericCrudActions("transferencias")
            },
            new()
            {
                Key         = "receptions",
                DisplayName = "Recepciones",
                Route       = "/receptions",
                Description = "Manage incoming stock receptions",
                Icon        = "package-import",
                Module      = Domain.Module.Inventory,
                IsMenu = true,
                CreatedAt   = DateTime.UtcNow,
                AvailableActions = GetGenericCrudActions("recepciones")
            },

            // ── Sales ──────────────────────────────────────────
            new()
            {
                Key         = "pos",
                DisplayName = "POS",
                Route       = "/pos",
                Description = "Process sales at point of sale",
                Icon        = "cash-register",
                Module      = Domain.Module.Sales,
                IsMenu = true,
                CreatedAt   = DateTime.UtcNow,
                // El POS suele requerir acciones más operativas, te dejo estas de base:
                AvailableActions = [
                    new() { Key = "read", DisplayName = "Acceso al POS", Description = "Permite abrir la pantalla de caja y buscar productos." },
                    new() { Key = "create", DisplayName = "Procesar Venta", Description = "Permite cobrar y emitir comprobantes de venta." },
                    new() { Key = "update", DisplayName = "Modificar Carrito", Description = "Permite aplicar descuentos o alterar líneas de venta en curso." }
                ]
            },
            new()
            {
                Key         = "sales",
                DisplayName = "Ventas",
                Route       = "/sales",
                Description = "View and manage sales records",
                Icon        = "receipt",
                Module      = Domain.Module.Sales,
                IsMenu = true,
                CreatedAt   = DateTime.UtcNow,
                AvailableActions = [
                    new() { Key = "read", DisplayName = "Ver Ventas", Description = "Permite listar el historial de ventas realizadas." },
                    new() { Key = "refund", DisplayName = "Devoluciones", Description = "Permite realizar notas de crédito o devoluciones de productos." },
                    new() { Key = "void_invoice", DisplayName = "Anular Facturas", Description = "Permite la cancelación/anulación total de un comprobante emitido." },
                    new() { Key = "export", DisplayName = "Exportar Reportes", Description = "Permite descargar listados de ventas en formatos externos (Excel/PDF)." }
                ]
            },
        };

        await context.Features.AddRangeAsync(features);
        await context.SaveChangesAsync();
    }
}