using Common.Contracts.authentication;
using Common.Contracts.branches;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Domain.Inventory;

namespace Module.Inventory.Application.UseCases.StockMovements.Get;

public class ListStockMovements(
    IInvDbContext context,
    ICurrentUser currentUser,
    IUserIntegrationService userIntegrationService,
    IBranchService branchService)
{
    public async Task<Result<PagedResultDto<ListStockMovementDto>>> Execute(
        Guid productVariantId,
        StockMovementQueryDto queryDto)
    {
        var currentBranchId = currentUser.BranchId;

        IQueryable<StockMovement> query = context.StockMovements
            .Where(sm => sm.ProductVariantId == productVariantId
                      && sm.BranchId == currentBranchId);

        var totalCount = await query.CountAsync();

        var rawMovements = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .ApplyPagination(queryDto)
            .Select(sm => new
            {
                sm.Id,
                sm.CreatedAt,
                sm.MovementType,
                sm.Quantity,
                sm.BranchId,
                sm.UserId,
                sm.Notes,
                sm.TransferToBranchId,
                ReferenceId = sm.ReferenceId.HasValue
                    ? sm.ReferenceId.Value.ToString()
                    : null
            })
            .ToListAsync();

        if (rawMovements.Count == 0)
            return new PagedResultDto<ListStockMovementDto>
            {
                TotalCount = totalCount,
                Items = [],
                Page = queryDto.Page,
                PageSize = queryDto.PageSize
            };

        var branchIds = rawMovements
            .Select(sm => sm.BranchId)
            .Concat(rawMovements
                .Where(sm => sm.TransferToBranchId.HasValue)
                .Select(sm => sm.TransferToBranchId!.Value))
            .Distinct()
            .ToList();

        var userIds = rawMovements
            .Select(sm => sm.UserId)
            .Distinct()
            .ToList();

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        if (!branchesResult.IsSuccess)
            return ListStockMovementsErrors.BranchLookupFailed;

        var usersResult = await userIntegrationService.GetUsersByIds(userIds);
        if (!usersResult.IsSuccess)
            return ListStockMovementsErrors.UserLookupFailed;

        var branches = branchesResult.Value.ToDictionary(b => b.Id, b => b.Name);
        var users = usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");

        var dtos = rawMovements.Select(sm => new ListStockMovementDto
        {
            Id = sm.Id,
            CreatedAt = sm.CreatedAt,
            MovementType = sm.MovementType,
            Quantity = sm.Quantity,
            UserName = users.GetValueOrDefault(sm.UserId) ?? "Unknown",
            BranchName = branches.GetValueOrDefault(sm.BranchId) ?? "Unknown",
            Notes = sm.Notes,
            TransferToBranchName = sm.TransferToBranchId.HasValue
                ? branches.GetValueOrDefault(sm.TransferToBranchId.Value) ?? "Unknown"
                : null,
            ReferenceId = sm.ReferenceId
        }).ToList();

        return new PagedResultDto<ListStockMovementDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = queryDto.Page,
            PageSize = queryDto.PageSize
        };
    }
}
