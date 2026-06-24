using Common.Utilities;
using module.Auth.dtos.Users;

namespace module.Auth.Features.Autentication;

public class RegisterDefaultUser(RegisterUser registerUser)
{
    public async Task<Result<bool>> Execute(RegisterUserDto dto)
    {
        //  var roleResult = await registerUser.GetDefaultUserRole();
        // if (!roleResult.IsSuccess)
        //     return roleResult.Error!;
        //
        // user.Status = UserStatus.PendingVerification; //this should be based on the settings or always be this way? not sure
        // user.UserBranchRoles = new List<UserBranchRole>
        // {
        //     new UserBranchRole { RoleId = Guid.Empty }
        // };
        //
        // return await registerUser.Execute(user, dto.Password);
        return new Error("NOT IMPMLEMENTED", "xd");
    }

}
