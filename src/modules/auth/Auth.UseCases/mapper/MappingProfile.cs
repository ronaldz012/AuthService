using System;
using Auth.Contracts.Dtos.Roles;
using Auth.Contracts.Dtos.Users;
using Auth.Data.Entities;
using Branches.Contracts.Dtos;
using Mapster;

namespace Auth.UseCases.mapper;

public class MappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateRoleDto, Role>();
        config.NewConfig<RoleFeaturePermissionDto, RoleFeaturePermission>();
        config.NewConfig<RegisterUserDto, User>();
        config.NewConfig<User, UserDetailsDto>();
        config.NewConfig<CreateUserDto, User>();
        
        //EXTERNAL MAPPING/////////////////////////
        //config.NewConfig<BranchDto, AvailableBranchesDto>(); //BranchDto es de otro modulo
    }
}
