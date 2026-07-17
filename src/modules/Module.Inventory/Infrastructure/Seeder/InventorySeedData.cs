using Module.Inventory.Domain.Products;

namespace Module.Inventory.Infrastructure.Seeder;

public static class InventorySeedData
{
    public record VariantSeed(string Color, string Size, decimal Price, int InitialStock, decimal UnitCost);
    public record ProductSeed(string Name, string Description, string Category, string Brand, Gender Gender, List<VariantSeed> Variants);
    public record BrandSeed(string Name, string Prefix);

    public static readonly string[] Colors = ["Negro", "Blanco", "Rojo", "Azul"];

    public static readonly string[] Categories = ["Zapatillas", "Casual", "Formales", "Accesorios"];

    public static readonly BrandSeed[] Brands =
    [
        new("Nike", "NIK"),
        new("Adidas", "ADI"),
        new("Puma", "PUM"),
    ];

    public static readonly ProductSeed[] Products =
    [
        new("Air Max 90", "Classic sneaker", "Zapatillas", "Nike", Gender.Unisex,
        [
            new("Negro", "42", 120m, 15, 80m),
            new("Blanco", "42", 120m, 12, 80m),
        ]),
        new("Revolution 7", "Running shoe", "Zapatillas", "Nike", Gender.Unisex,
        [
            new("Rojo", "40", 90m, 20, 60m),
            new("Azul", "41", 90m, 18, 60m),
        ]),
        new("Forum Low", "Classic casual shoe", "Casual", "Adidas", Gender.Male,
        [
            new("Blanco", "43", 110m, 10, 75m),
        ]),
    ];
}
