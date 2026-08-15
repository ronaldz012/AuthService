namespace Common.Contracts.authentication;

public record ActorContext(
    Guid TenantId,
    Guid UserId,
    string FullName,
    Guid BranchId,
    IReadOnlyList<Guid> BranchIds);

public static class ActorContextExtensions
{
    public static ActorContext ToActorContext(this ICurrentUser currentUser)
        => new(
            currentUser.TenantId,
            currentUser.UserId,
            currentUser.FullName,
            currentUser.BranchId,
            currentUser.BranchIds);
}