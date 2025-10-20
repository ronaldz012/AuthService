using System;
using Auth.UseCases.Menus;

namespace Auth.UseCases;

public record MenuUseCases(
    AddMenu AddMenu,
    GetMenu GetMenu,
    GetAllMenus GetAllMenus,
    UpdateMenu UpdateMenu,
    DeleteMenu DeleteMenu
);