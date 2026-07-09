namespace Module.Auth.Application.UseCases.Users;

public record UserUserCases(
    GetAllUsers.GetAllUsers GetAllUsers,
    CreateUser.CreateUser CreateUser,
    CreateTenantAdmin.CreateTenantAdmin CreateTenantAdmin,
    UpdateUserStatus.UpdateUserStatus UpdateUserStatus,
    UpdateUser.UpdateUser UpdateUser,
    GetUserDetails.GetUserDetails GetUserDetails,
    ToggleUserType.ToggleUserType ToggleUserType);
