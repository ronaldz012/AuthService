using System;
using Auth.Data.Entities;
using Auth.Dtos.Modules;
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

    }
}
