namespace DutchMetar.Core.Domain.Exceptions;

public class EntityNotFoundException : DutchMetarException
{
    public override string Message => field ?? base.Message;

    public EntityNotFoundException(string entityName, string entityKey)
    {
        Message = $"Entity {entityName} with key {entityKey} not found.";
    }
}