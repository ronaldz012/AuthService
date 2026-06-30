using Module.Auth.Application.UseCases.Autentication.Login;
using Module.Auth.Application.UseCases.Autentication.PublicLogin;
using Module.Auth.Application.UseCases.Autentication.VerifiUser;
using Module.Auth.Application.UseCases.Users.Pending;

namespace Module.Auth.Application.UseCases.Autentication;

public record AutenticationUseCases(RegisterDefaultUser RegisterDefaultUser,
                            RegisterUser RegisterUser,
                            Module.Auth.Application.UseCases.Autentication.Login.Login Login,
                             VerifyUser VerifyUser,
                             CompletePublicRegister CompletePublicRegister);