using Auth.Contracts.Interfaces;
using Branches.Contracts;
using Inventory.Contracts.Dtos.Transfers;
using Inventory.Data.Entities.Transfers;
using Inventory.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Result;

namespace Inventory.UseCases.Transfers;

public class ListStockTransfers(InvDbContext context ,IUserIntegrationService userIntegrationService, ICurrentUser currentUser, IBranchService branchService)
{
    public async Task<Result<PagedResultDto<ListStockTransferDto>>> Execute(StockTransferQueryDto queryDto)
    {
        var currentBranchId = currentUser.BranchIds[0];

        IQueryable<StockTransfer> query = context.StockTransfers.AsQueryable();

        if (queryDto.Status is { Count: > 0 })
            query = query.Where(st => queryDto.Status.Contains(st.Status));

        query = queryDto.Direction switch
        {
            TransferDirection.Inbound => query.Where(st => st.ToBranchId == currentBranchId),
            TransferDirection.Outbound => query.Where(st => st.FromBranchId == currentBranchId),
            _ => query.Where(st => st.ToBranchId == currentBranchId
                                   || st.FromBranchId == currentBranchId)
        };

        var (pagedQuery, totalCount) = query.ApplyFilters(queryDto);

        // Materializar solo lo necesario antes de llamadas externas
        var rawTransfers = await pagedQuery
            .Select(st => new
            {
                st.Id,
                st.FromBranchId,
                st.ToBranchId,
                st.RequestedByUserId,
                st.Status,
                st.CreatedAt,
                st.ResolvedAt,
                TotalItems = st.Items.Count,
                TotalQuantity = st.Items.Sum(i => i.QuantityRequested)
            })
            .ToListAsync();

        if (rawTransfers.Count == 0)
            return new PagedResultDto<ListStockTransferDto>
            {
                TotalCount = totalCount,
                Items = [],
                Page = queryDto.GetPageValue(),
                PageSize = queryDto.GetPageSizeValue()
            };

        // Llamadas externas con IDs reales
        var branchIds = rawTransfers
            .SelectMany(t => new[] { t.FromBranchId, t.ToBranchId })
            .Distinct()
            .ToList();

        var userIds = rawTransfers
            .Select(t => t.RequestedByUserId)
            .Distinct()
            .ToList();

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        if (!branchesResult.IsSuccess)
            return new Error("INTERNAL_ERROR", branchesResult.Error.Message);

        var usersResult = await userIntegrationService.GetUsersByIds(userIds);
        if (!usersResult.IsSuccess)
            return new Error("INTERNAL_ERROR", "Failed to resolve user names");

        var branches = branchesResult.Value.ToDictionary(b => b.Id, b => b.Name);
        var users = usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");

        var dtos = rawTransfers.Select(t =>
        {
            var isOutbound = t.FromBranchId == currentBranchId;
            var counterpartId = isOutbound ? t.ToBranchId : t.FromBranchId;

            return new ListStockTransferDto
            {
                Id = t.Id,
                Direction = isOutbound ? TransferDirection.Outbound : TransferDirection.Inbound,
                CounterpartBranchName = branches.GetValueOrDefault(counterpartId) ?? "Unknown",
                RequesterName = users.GetValueOrDefault(t.RequestedByUserId) ?? "Unknown",
                Status = t.Status,
                TotalItems = t.TotalItems,
                TotalQuantity = t.TotalQuantity,
                CreatedAt = t.CreatedAt,
                ResolvedAt = t.ResolvedAt
            };
        }).ToList();

        return new PagedResultDto<ListStockTransferDto>{
            TotalCount = totalCount,
            Items =  dtos,
            Page = queryDto.GetPageValue(),
            PageSize = queryDto.GetPageSizeValue()
        };


    }
}