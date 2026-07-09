namespace Module.Auth.Application.UseCases.Users.CreateTenantAdmin;

public record CreateTenantAdminResponse(Guid UserId, string SetupUrl, bool EmailSent);
