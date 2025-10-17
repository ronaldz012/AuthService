namespace Shared.Domain;

// ✅ MEJOR ENFOQUE - Composición
public interface ICreatedAt 
{
    DateTime CreatedAt { get; set; }
}

public interface IUpdatedAt 
{
    DateTime? UpdatedAt { get; set; }
}

public interface ISoftDelete 
{
    DateTime? DeletedAt { get; set; }
    int? DeletedBy { get; set; }
}

public interface ICreatedBy 
{
    int CreatedBy { get; set; }
}

public interface IUpdatedBy 
{
    int? UpdatedBy { get; set; }
}

