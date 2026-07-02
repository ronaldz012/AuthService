namespace Module.Auth.Application.UseCases.Autentication.VerifyToken;

public class VerifyTokenResponse
{
    public bool Valid { get; set; }
    public string? Email { get; set; }
}
