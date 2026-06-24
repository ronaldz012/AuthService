namespace Module.Auth.Application.UseCases.Autentication.Login;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
