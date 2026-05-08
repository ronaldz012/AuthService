using Auth.Contracts.Interfaces;
using Branches.Contracts;
using Inventory.Contracts.Dtos.Transfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Common.Result;
using Inventory.Data;

namespace Inventory.UseCases.Transfers;

public class StockTransferDetails(InvDbContext context, IBranchService branchService, IUserIntegrationService userService, ICurrentUser currentUser)
{
    public async Task<Result<StockTransferDetailDto>> Execute(Guid stockTransferId)
    {
        var currentBranchIds = currentUser.BranchIds[0];
        var transfer = await context.StockTransfers
            .Include(st => st.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(x => x.Id == stockTransferId);

        if (transfer == null)
            return new Error("NOT_FOUND", "StockTransfer not found");

        List<Guid> branchIds = [transfer.FromBranchId, transfer.ToBranchId];
        List<Guid> userIds = transfer.ResolvedByUserId.HasValue
            ? [transfer.RequestedByUserId, transfer.ResolvedByUserId.Value]
            : [transfer.RequestedByUserId];

        var branchesResult = await branchService.GetBranchesByIds(branchIds);
        if (!branchesResult.IsSuccess)
            return new Error("NOT_FOUND", $"Branches not found: {branchesResult.Error.Message}");

        var usersResult = await userService.GetUsersByIds(userIds);
        if (!usersResult.IsSuccess)
            return new Error("NOT_FOUND", "Users not found");

        var branches = branchesResult.Value.ToDictionary(b => b.Id, b => b.Name);
        var users = usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");

        var direction = transfer.FromBranchId == currentBranchIds ? TransferDirection.Outbound : TransferDirection.Inbound;

        return new StockTransferDetailDto
        {
            Id = transfer.Id,
            Direction = direction,
            FromBranchName = branches.GetValueOrDefault(transfer.FromBranchId) ?? "Unknown",
            ToBranchName = branches.GetValueOrDefault(transfer.ToBranchId) ?? "Unknown",
            RequesterName = users.GetValueOrDefault(transfer.RequestedByUserId) ?? "Unknown",
            ResolverName = users.GetValueOrDefault(transfer.ResolvedByUserId ?? Guid.Empty) ?? "Unknown",
            Status = transfer.Status,
            Notes = transfer.Notes,
            CreatedAt = transfer.CreatedAt,
            ResolvedAt = transfer.ResolvedAt,
            Items = transfer.Items.Select(i => new StockTransferItemDetailDto
            {
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductVariant.Product.Name,
                VariantDescription = i.ProductVariant.Description,
                Sku = i.ProductVariant.Sku,
                Size = i.ProductVariant.Size,
                Color = i.ProductVariant.Color,
                QuantityRequested = i.QuantityRequested
            }).ToList()
        };
    }
}