using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.Extensions;

public class GenericPaginationQueryDto : IValidatableObject
{
    /// <summary>
    /// Indica si la consulta debe ser paginada. El valor predeterminado es <c>true</c>.
    /// </summary>
    public bool? IsPaged { get; set; }

    /// <summary>
    /// El número de página a recuperar. El valor predeterminado es 1.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// El número de elementos por página. El valor predeterminado es 10.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Un filtro de texto general para la búsqueda en múltiples campos.
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// El nombre del campo por el cual se ordenarán los resultados.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// La dirección de la ordenación ("asc" para ascendente o "desc" para descendente). El valor predeterminado es "desc".
    /// </summary>
    public string? SortDirection { get; set; }

    /// <summary>
    /// El valor del campo a filtrar.
    /// </summary>
    public string? FieldValue { get; set; }

    /// <summary>
    /// El nombre del campo por el cual se aplicará el filtro.
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Obtiene el valor de <c>IsPaged</c>, usando <c>true</c> si es nulo.
    /// </summary>
    /// <returns>El valor booleano de <c>IsPaged</c> o <c>true</c>.</returns>
    public bool GetIsPagedValue() => IsPaged ?? true;

    /// <summary>
    /// Obtiene el valor de <c>Page</c>, usando 1 si es nulo.
    /// </summary>
    /// <returns>El número de página o 1.</returns>
    public int GetPageValue() => Page ?? 1;

    /// <summary>
    /// Obtiene el valor de <c>PageSize</c>, usando 10 si es nulo.
    /// </summary>
    /// <returns>El tamaño de la página o 10.</returns>
    public int GetPageSizeValue() => PageSize ?? 10;

    /// <summary>
    /// Obtiene el valor de <c>SortDirection</c>, usando "desc" si es nulo.
    /// </summary>
    /// <returns>La dirección de ordenación o "desc".</returns>
    public string GetSortDirectionValue() => SortDirection ?? "desc";

    /// <summary>
    /// Valida los valores de paginación para asegurar que sean correctos y seguros.
    /// </summary>
    /// <param name="validationContext">El contexto de validación.</param>
    /// <returns>Una colección de resultados de validación.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GetIsPagedValue())
        {
            if (Page.HasValue && Page.Value <= 0)
                yield return new ValidationResult("Page debe ser mayor que 0.", new[] { nameof(Page) });

            if (PageSize.HasValue && PageSize.Value <= 0)
                yield return new ValidationResult("PageSize debe ser mayor que 0.", new[] { nameof(PageSize) });
            
            if (PageSize.HasValue && PageSize.Value > 100)
                yield return new ValidationResult("PageSize no puede ser mayor que 100.", new[] { nameof(PageSize) });
        }
    }
}