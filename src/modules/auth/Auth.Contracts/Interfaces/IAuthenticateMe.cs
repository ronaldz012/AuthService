using System;
using Auth.Contracts.Dtos.Users;
using Common.Result;

namespace Auth.Contracts.Interfaces;

public interface IAuthenticateMe
{
    Task<Result<SuccessLoginDto>> Execute();

}
