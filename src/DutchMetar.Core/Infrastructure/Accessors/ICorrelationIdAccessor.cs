namespace DutchMetar.Core.Infrastructure.Accessors;

/// <summary>
/// Contains the correlation <see cref="Guid"/> of the current scope
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Correlation ID of the current DI Scope
    /// </summary>
    public Guid CorrelationId { get; }
}