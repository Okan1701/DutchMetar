namespace DutchMetar.Core.Infrastructure.Accessors;

public class SimpleCorrelationIdAccessor : ICorrelationIdAccessor
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
}