using System;
using Auth.Data.Entities;
using Auth.Dtos.Modules;
using Auth.Dtos.Roles;
using Auth.Dtos.Users;
using Mapster;

namespace Auth.UseCases.mapper;

public class MappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateMenuDto, Menu>()
            .IgnoreNullValues(true);


        config.NewConfig<UpdateMenuDto, Menu>()
            .IgnoreNullValues(true);

        config.NewConfig<CreateModuleDto, Module>()
            .IgnoreNullValues(true);
        config.NewConfig<CreateMenuForModuleDto, Menu>()
            .IgnoreNullValues(true);

        config.NewConfig<Module, ModuleDto>();
        config.NewConfig<Module, ModuleDetailsDto>();
        config.NewConfig<Menu, MenuDto>();

        config.NewConfig<CreateRoleDto, Role>();
        config.NewConfig<RoleModulePermissionDto, RoleModulePermission>();

        config.NewConfig<Role, RoleDetailsDto>()
        .Map(dest => dest.ModulePermissions,
         src => src.RoleModulePermissions.Select(rmp => new ModulePermisionsDto
         {
             ModuleId = rmp.ModuleId,
             ModuleName = rmp.Module.Name,
             CanCreate = rmp.CanCreate,
             CanRead = rmp.CanRead,
             CanUpdate = rmp.CanUpdate,
             CanDelete = rmp.CanDelete
         }).ToList());
         
         config.NewConfig<RegisterUserDto, User>()
         .Map(dest => dest.UserRoles,
          src => src.RoleIds.Select(roleId => new UserRole
          {
              RoleId = roleId
          }).ToList());
    }
}
