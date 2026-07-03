using Common.Utilities;

namespace Module.Auth.Application.UseCases.Features;

public static class GetFeatureErrors
{
    public static readonly Error FeatureNotFound = new(ErrorCode.NotFound, "Module not found");
}
