namespace Common.Contracts.inventory;

public static class DefaultCatalogTemplates
{
    public static readonly DefaultCatalogTemplate Basic = new()
    {
        Colors = ["Azul", "Celeste", "Plomo", "Acero", "Negro", "Marengo", "Menta", "Plata", "Verde Oscuro", "Arena", "Beige", "Café", "Guindo", "Oliva", "Blanco"],
        Sizes =
        [
            new("34", 1),
            new("35", 2),
            new("36", 3),
            new("37", 4),
            new("38", 5),
            new("39", 6),
            new("40", 7),
            new("41", 8),
            new("42", 9),
            new("43", 10),
            new("44", 11),
            new("45", 12),
            new("46", 13),
            new("47", 14),
            new("48", 15),
            new("49", 16),
            new("50", 17),
            new("51", 18),
            new("52", 19),
            new("53", 20),
            new("54", 21),
            new("XS", 22),
            new("S", 23),
            new("M", 24),
            new("L", 25),
            new("XL", 26),
            new("XXL", 27),
            new("XXXL", 28),
        ],
        Brands = [],
        Categories =
        [
            new("Pantalón", "Prendas de vestir para piernas"),
            new("Chaqueta", "Prendas de abrigo para torso"),
            new("Camisa", "Prendas con botones, formales o casuales"),
            new("Polera", "Prendas de vestir de tela"),
            new("Zapatillas", "Calzado deportivo o casual"),
            new("Accesorios", "Complementos y accesorios"),
        ],
    };
}