using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Providers.ToggleProvider;

public static class ToggleProviderErrors
{
    public static readonly Error ProviderNotFound = new(ErrorCode.NotFound, "Provider not found");
}
