# Integration Services

> **Scope:** Module boundary interfaces (defined in Common)
> **When to use:** Writing integration services between modules (e.g., InventoryIntegrationService, SalesIntegrationService)

Integration Services (which implement interfaces defined in the `Common` project) act as the module boundary. Their purpose is to coordinate data access and translate pure domain rules into the application's control flow pattern (`Result<T>`).

## 1. Integration Service Responsibilities
- **Orchestrate persistence:** Load entities from their own module's database (using its respective `DbContext`).
- **Query the domain:** Use the entity's *Tester* methods to verify feasibility.
- **Map errors:** Translate domain validation failures into strongly-typed `Error` objects with `ErrorCode`.
- **SaveChangesAsync policy:** See table below — depends on whether the caller owns the transaction.

## 2. SaveChangesAsync Policy

| Scenario | SaveChangesAsync? |
|---|---|
| Called from another module's use case (cross-module tx) | **NO** — caller controls transaction |
| Called standalone (e.g., from a controller or seeder) | **YES** — owns its own transaction |

For dual-use integration services, create an overload or pass a `bool saveChanges = true` parameter.

## 3. Standard Service Structure

Services must always return a `Result<T>` or `Result<bool>` to prevent exception leaks to the consuming use cases.

```csharp
public class InventoryIntegrationService(IInvDbContext context) : IInventoryIntegrationService
{
    public async Task<Result<bool>> DeductStock(
        List<StockDeductionDto> deductions, Guid branchId, Guid userId, Guid referenceId)
    {
        var variantIds = deductions.Select(d => d.ProductVariantId).ToList();

        // 1. Load data into memory (Single SQL query)
        var variants = await context.ProductVariants
            .Include(pv => pv.BranchInventories.Where(bi => bi.BranchId == branchId))
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync();

        foreach (var deduction in deductions)
        {
            var pv = variants.FirstOrDefault(v => v.Id == deduction.ProductVariantId);
            if (pv == null)
            {
                return new Error(ErrorCode.NotFound, $"Product variant {deduction.ProductVariantId} not found.");
            }

            // 2. Query the Domain (Tester)
            if (!pv.HasSufficientStock(deduction.Quantity, branchId))
            {
                // Translate business failure into a typed, friendly Error
                return new Error(ErrorCode.Conflict, $"Insufficient stock for product {pv.Sku}.");
            }

            // 3. Execute the action in the Domain (Doer)
            pv.SellStock(deduction.Quantity, branchId, userId, referenceId);
        }

        // 4. DO NOT call SaveChangesAsync here when participating in a cross-module transaction.
        //    The caller owns the transaction and calls SaveChangesAsync once.
        return true; // Implicit conversion to successful Result
    }
}
```
