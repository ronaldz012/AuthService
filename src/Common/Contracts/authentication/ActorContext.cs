namespace Common.Contracts.authentication;

public record ActorContext(
    Guid TenantId,
    Guid UserId,
    string FullName,
    Guid BranchId,
    IReadOnlyList<Guid> BranchIds);

