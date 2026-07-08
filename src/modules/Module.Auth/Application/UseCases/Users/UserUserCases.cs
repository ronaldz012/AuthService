namespace Module.Auth.Application.UseCases.Users;

public record UserUserCases(
    GetAllUsers.GetAllUsers GetAllUsers,
    CreateUser.CreateUser CreateUser,
    UpdateUserStatus.UpdateUserStatus UpdateUserStatus,
    UpdateUser.UpdateUser UpdateUser);
