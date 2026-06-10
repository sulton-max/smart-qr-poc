namespace SmartQr.Common.Domain.Common.Entities;

/// <summary>Base contract for all domain entities.</summary>
public interface IEntity
{
    /// <summary>Gets or sets the UUID primary key of the entity.</summary>
    Guid Id { get; set; }

    /// <summary>Gets the storage identifier (table name) for this entity type.</summary>
    static abstract string TableName { get; }
}
