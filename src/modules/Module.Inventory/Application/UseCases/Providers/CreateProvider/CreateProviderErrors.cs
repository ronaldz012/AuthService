using Common.Utilities;

namespace Module.Inventory.Application.UseCases.Providers.CreateProvider;

public static class CreateProviderErrors
{
    public static readonly Error ProviderNameAlreadyExists = new(ErrorCode.Duplicate, "There is already a provider with that name");
}
