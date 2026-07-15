# Controller Patterns

> **Scope:** System.Api
> **When to use:** Creating or modifying API controllers in any module

## Class Structure
- Primary constructor with the grouping record(s)
- `[Route("api/[controller]")]`, `[ApiController]`, `[Tags("...")]`, `[Authorize]`
- Class is `public` and non-static

```csharp
[Route("api/[controller]")]
[ApiController]
[Tags("Inventory | ProductVariants")]
[Authorize]
public class ProductVariantController(ProductVariantUseCases useCases) : ControllerBase
{
}
```

## Endpoint Methods
- Delegate to `useCase.Execute().ToValueOrProblemDetails()`
- Return `Task<IActionResult>`
- Use the `ToValueOrProblemDetails()` extension from `System.Api.Result`

```csharp
[HttpGet("{id:guid}/movements")]
public async Task<IActionResult> GetMovements([FromRoute] Guid id, [FromQuery] SomeQueryDto query)
{
    return await useCases.ListSomething.Execute(id, query).ToValueOrProblemDetails();
}
```

## Parameter Binding
| Parameter type | Attribute | Example |
|---|---|---|
| Route ID | `[FromRoute] Guid id` | `{id:guid}` |
| Query/pagination DTO | `[FromQuery] SomeQueryDto query` | `?page=1&pageSize=10` |
| Command body | `[FromBody] SomeDto dto` | POST/PUT body |
| Simple query param | `[FromQuery] string request` | `?request=abc` |

## URL Conventions
- `GET` list: `[HttpGet]` — optionally with `{id:guid}` prefix for sub-resources
- `GET` detail: `[HttpGet("{id:guid}/details")]`
- `GET` sub-resource list: `[HttpGet("{id:guid}/movements")]`
- `POST` create: `[HttpPost]` or `[HttpPost("{parentId:guid}")]`
- `PUT` update: `[HttpPut("{id:guid}")]`
- `PATCH` partial: `[HttpPatch("{id:guid}")]`
- `DELETE` soft: `[HttpDelete("{id:guid}")]`

## Adding a Second Grouping Record
When a controller needs use cases from a different domain, inject the additional record:

```csharp
public class ProductVariantController(
    ProductVariantUseCases useCases,
    StockMovementUseCases stockMovementUseCases) : ControllerBase
```

Use the named field for each domain:

```csharp
return await stockMovementUseCases.ListMovements.Execute(id, query)
    .ToValueOrProblemDetails();
```
