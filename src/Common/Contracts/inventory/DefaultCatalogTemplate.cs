using Microsoft.EntityFrameworkCore;

namespace Common.Contracts.inventory;

[Owned]
public class DefaultCatalogTemplate
{
    public List<string> Colors { get; set; } = [];
    public List<DefaultSizeTemplate> Sizes { get; set; } = [];
    public List<DefaultBrandTemplate> Brands { get; set; } = [];
    public List<DefaultCategoryTemplate> Categories { get; set; } = [];
}

[Owned]
public record DefaultSizeTemplate(string Name, int SortOrder);

[Owned]
public record DefaultBrandTemplate(string Name, string Prefix, string Description);

[Owned]
public record DefaultCategoryTemplate(string Name, string Description);