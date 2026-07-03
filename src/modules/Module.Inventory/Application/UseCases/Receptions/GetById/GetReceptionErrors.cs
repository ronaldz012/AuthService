using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Receptions.GetById;

public static class GetReceptionErrors
{
    public static readonly Error ReceptionNotFound = new(ErrorCode.NotFound, "Reception not found");
}
