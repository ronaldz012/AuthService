namespace module.Auth.Features.Features;

public record FeatureUseCases(
    CreateFeature CreateFeature,
    GetFeature GetFeature,
    ListFeatures ListFeatures
);