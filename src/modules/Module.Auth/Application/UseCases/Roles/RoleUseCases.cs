
using Module.Auth.Application.UseCases.Roles.GetById;
using Module.Auth.Application.UseCases.Roles.Create;

namespace Module.Auth.Application.UseCases.Roles;

public record RoleUseCases
(
    AddRole AddRole,
    GetRoleById GetRoleById,
    GetRoles.GetRoles GetRoles
);
