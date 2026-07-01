
using Module.Auth.Application.UseCases.Roles.GetById;
using Module.Auth.Application.UseCases.Roles.Create;
using Module.Auth.Application.UseCases.Roles.GetRoles;

namespace Module.Auth.Application.UseCases.Roles;

public record RoleUseCases
(
    AddRole AddRole,
    GetRole GetRole,
    GetRoles.GetRoles GetRoles
);
