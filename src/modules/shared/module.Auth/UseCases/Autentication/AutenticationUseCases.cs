namespace module.Auth.Features.Autentication;

public record AutenticationUseCases(RegisterDefaultUser RegisterDefaultUser,
                            RegisterUser RegisterUser,
                             Login Login,
                            IAuthenticateMe AutenticateMe,
                             VerifyUser VerifyUser,
                             CompletePublicRegister CompletePublicRegister);