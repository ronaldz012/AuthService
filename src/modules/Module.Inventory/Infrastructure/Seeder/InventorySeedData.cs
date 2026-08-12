using Module.Inventory.Domain.Products;

namespace Module.Inventory.Infrastructure.Seeder;

public static class InventorySeedData
{
    public record VariantSeed(string Color, string Size, decimal Price, int InitialStock, decimal UnitCost);
    public record ProductSeed(string Name, string Description, string Category, string Brand, Gender Gender, List<VariantSeed> Variants);
    public record BrandSeed(string Name, string Prefix);
    public record ProviderSeed(string Name, string ContactName, string Email, string PhoneNumber, string Address);
    public record SizeSeed(string Name, int SortOrder);

    public static readonly string[] Colors = ["Negro", "Blanco", "Rojo", "Azul"];

    public static readonly SizeSeed[] Sizes =
    [
        new("36", 1),
        new("37", 2),
        new("38", 3),
        new("39", 4),
        new("40", 5),
        new("41", 6),
        new("42", 7),
        new("43", 8),
        new("44", 9),
        new("45", 10),
        new("XS", 11),
        new("S", 12),
        new("M", 13),
        new("L", 14),
        new("XL", 15),
        new("XXL", 16),
    ];

    public static readonly string[] Categories = ["Zapatillas", "Casual", "Formales", "Accesorios"];

    public static readonly BrandSeed[] Brands =
    [
        new("Nike", "NIK"),
        new("Adidas", "ADI"),
        new("Puma", "PUM"),
    ];

    public static readonly ProviderSeed[] Providers =
    [
        new("Shoes Import S.A.", "Carlos Mendoza", "ventas@shoesimport.com", "+595 981 234 567", "Av. Mariscal López 1045, Asunción"),
        new("Distribuidora Deportiva", "Laura Ferreira", "contacto@distdeportiva.com", "+595 971 456 789", "Ruta Transchaco Km 12, Asunción"),
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
