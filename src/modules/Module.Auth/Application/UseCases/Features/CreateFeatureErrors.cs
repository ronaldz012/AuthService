using Common.Utilities;

namespace Module.Auth.Application.UseCases.Features;

public static class CreateFeatureErrors
{
    public static readonly Error FeatureAlreadyExists = new(ErrorCode.Duplicate, "Ya existe un módulo con ese nombre");
}
