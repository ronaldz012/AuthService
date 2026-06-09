using System.Reflection.Metadata.Ecma335;
using Auth.Contracts.Interfaces;
using Branches.Contracts;
using Common.Extensions;
using Common.Result;
using Inventory.Contracts.Dtos.StockMovements;
using Inventory.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UseCases.StockMovements;

 public class ListStockMovementsUc(InvDbContext context, IUserIntegrationService userService, IBranchService branchService)
{
     public async Task<Result<PagedResultDto<ListStockMovementDto>>> Execute(Guid productVariantId ,StockMovementsQuery request)
    {
        var query = context.StockMovements.AsQueryable();
        query = query.Where(sm => sm.ProductVariantId == productVariantId);


        var (paginatedQuery, totalCount) = query.ApplyFilters(request);


        var movements = await paginatedQuery.ToListAsync();

            var userIds = movements
        .Select(x => x.UserId)
        .Distinct()
        .ToList();

        var branchIds = movements
            .Select(x => x.BranchId)
            .Concat(
                movements
                    .Where(x => x.TransferToBranchId.HasValue)
                    .Select(x => x.TransferToBranchId!.Value)
            )
            .Distinct()
            .ToList();

        var usersTask = userService.GetUsersByIds(userIds);
        var branchesTask = branchService.GetBranchesByIds(branchIds);

        await Task.WhenAll(usersTask, branchesTask);

        var usersResult = await usersTask;
        if(!usersResult.IsSuccess)
            return usersResult.Error;


        var branchesResult = await branchesTask;
        if(!branchesResult.IsSuccess)
            return branchesResult.Error;

        var users = usersResult.Value.ToDictionary(x => x.Id);
        var branches = branchesResult.Value.ToDictionary(x => x.Id);

        var items = movements
        .Select(sm => new ListStockMovementDto
        {
            Id = sm.Id,

            CreatedAt = sm.CreatedAt,

            MovementType = sm.MovementType,

            Quantity = sm.Quantity,

            UserName =
                users.TryGetValue(sm.UserId, out var user)? user.FirstName + user.LastName : string.Empty,

            BranchName =
                branches.TryGetValue(sm.BranchId, out var branch)? branch.Name: string.Empty,

            TransferToBranchName =
                sm.TransferToBranchId.HasValue &&
                branches.TryGetValue(
                    sm.TransferToBranchId.Value,
                    out var transferBranch)
                        ? transferBranch.Name
                        : string.Empty,

            Notes = sm.Notes,

            StockTransferId = sm.ReferenceId
        })
        .ToList();


        return new PagedResultDto<ListStockMovementDto>
        {
            Items = items,
            TotalCount= totalCount,
            Page = request.GetPageValue(),
            PageSize = request.GetPageSizeValue()
        };
    }
}