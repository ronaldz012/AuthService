using Module.Inventory.Application.UseCases.Transfers.Cancel;
using Module.Inventory.Application.UseCases.Transfers.Create;
using Module.Inventory.Application.UseCases.Transfers.Get;
using Module.Inventory.Application.UseCases.Transfers.GetById;
using Module.Inventory.Application.UseCases.Transfers.Resolve;

namespace Module.Inventory.Application.UseCases.Transfers;

public record StockTransferUseCases(CreateStockTransfer CreateStockTransfer, 
    ResolveStockTransfer ResolveStockTransfer, 
    StockTransferDetails StockTransferDetails,
    CancelStockTransfer CancelStockTransfer,
    ListStockTransfers ListStockTransfers);