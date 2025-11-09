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
    /// <see cref="DateTime"/> when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// <see cref="DateTime"/> when this entity was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }
    
    /// <summary>
    /// ID of the request/session that created this entity.
    /// Can be useful for logging.
    /// </summary>
    public virtual Guid? CorrelationId { get; set; }
}