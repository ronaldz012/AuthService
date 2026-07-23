namespace Common.Domain;

public interface ICreatedAt
{
    DateTime CreatedAt { get; set; }
}

public interface IUpdatedAt
{
    DateTime? UpdatedAt { get; set; }
}

public interface ICreatedBy
{
    Guid CreatedBy { get; set; }
    string CreatedByName { get; set; }
}

public interface IUpdatedBy
{
    Guid? UpdatedBy { get; set; }
    string? UpdatedByName { get; set; }
}

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
    string? DeletedByName { get; set; }
}

public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}

