namespace DutchMetar.Core.Domain.Entities;

/// <summary>
/// Abstract entity with fields that all entities should contain.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Unique numeric identifier of the entity.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// <see cref="DateTimeOffset"/> when this entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// <see cref="DateTimeOffset"/> when this entity was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
    
    /// <summary>
    /// ID of the request/session that created this entity.
    /// Can be useful for logging.
    /// </summary>
    public virtual Guid? CorrelationId { get; set; }
}