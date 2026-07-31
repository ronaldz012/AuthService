using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Providers.UpdateProvider;

public static class UpdateProviderErrors
{
    public static readonly Error ProviderNotFound = new(ErrorCode.NotFound, "Provider not found");
    public static readonly Error ProviderNameAlreadyExists = new(ErrorCode.Duplicate, "There is already a provider with that name");
}
