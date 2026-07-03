using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Receptions.Create;

public static class CreateReceptionErrors
{
    public static readonly Error VariantsNotFound = new(ErrorCode.NotFound, "One or more variants not found");
}
