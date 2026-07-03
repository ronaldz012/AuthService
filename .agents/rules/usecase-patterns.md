# UseCase Patterns (Module.Inventory)

## Class Structure
- Plain C# class with **primary constructor** for DI (no MediatR)
- Single public method named `Execute` returning `Task<Result<TDto>>`
- Group use cases under domain folder: `UseCases/{Domain}/Get/`, `UseCases/{Domain}/Create/`, etc.

```csharp
public class ListSomething(IInvDbContext context, IServiceX serviceX)
{
    public async Task<Result<PagedResultDto<SomethingDto>>> Execute(Guid id, SomeQueryDto query)
    {
        // 1. Build IQueryable, apply filters
        // 2. Call query.ApplyFilters(dto) for pagination
        // 3. Materialize raw data with .Select().ToListAsync()
        // 4. Batch-resolve external names (branches, users)
        // 5. Map to DTOs and return PagedResultDto<T>
    }
}
```

## DTO Mapping
- **No AutoMapper** — manual LINQ `.Select()` projections in the `Execute` method
- Flat DTOs with primitives/strings only, no nested entities
- Enums mapped directly (e.g. `MovementType`, `ReceptionStatus`)

## Pagination
- Query DTO extends `GenericPaginationQueryDto`
- Call `.ApplyFilters(queryDto)` extension to get `(IQueryable<T> Query, int TotalCount)`
- Return `Result<PagedResultDto<TDto>>`

```csharp
var (pagedQuery, totalCount) = query.ApplyFilters(queryDto);
```

## External Lookups (Branch/User names)
1. `.Select()` to project only the IDs needed (don't include nav properties for external modules)
2. `.ToListAsync()` to execute the query
3. Batch-resolve via `IBranchService.GetBranchesByIds(ids)` / `IUserIntegrationService.GetUsersByIds(ids)`
4. `ToDictionary()` for O(1) lookups when mapping to DTOs
5. Handle failure results with error definitions

## Errors
- Static class per use case: `static class ListSomethingErrors`
- `static readonly Error` fields using `ErrorCode` enum
- One error per distinguishable failure mode

```csharp
public static class ListSomethingErrors
{
    public static readonly Error NotFound = new(ErrorCode.NotFound, "Entity not found");
    public static readonly Error BranchLookupFailed = new(ErrorCode.InternalError, "Failed to resolve branch names");
}
```

## Grouping Record
- Each domain area has a `record` bundling its use cases
- Registered as `Scoped` in DI alongside individual use cases

```csharp
public record SomethingUseCases(ListSomething ListSomething, GetSomething GetSomething);
```

## DI Registration (InvDependencyInjection.cs)
- Register both the grouping record and each individual use case as Scoped

```csharp
services.AddScoped<SomethingUseCases>()
    .AddScoped<ListSomething>()
    .AddScoped<GetSomething>();
```

## Branch Scoping
- Always filter queries by current user's branch: `query.Where(x => x.BranchId == currentUser.BranchIds[0])`
