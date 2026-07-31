using Module.Inventory.Application.UseCases.Providers.CreateProvider;
using Module.Inventory.Application.UseCases.Providers.GetProviders;
using Module.Inventory.Application.UseCases.Providers.ToggleProvider;
using Module.Inventory.Application.UseCases.Providers.UpdateProvider;

namespace Module.Inventory.Application.UseCases.Providers;

public record ProviderUseCases(
    CreateProviderUc CreateProvider,
    GetProviders.GetProviders GetProviders,
    UpdateProviderUc UpdateProvider,
    ToggleProviderUc ToggleProvider);
