using Module.Inventory.Application.UseCases.Providers.CreateProvider;
using Module.Inventory.Application.UseCases.Providers.GetProviders;
using Module.Inventory.Application.UseCases.Providers.Update;

namespace Module.Inventory.Application.UseCases.Providers;

public record ProviderUseCases(
    CreateProviderUc CreateProvider,
    GetProviders.GetProviders GetProviders,
    UpdateProvider UpdateProvider);
