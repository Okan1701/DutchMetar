namespace DutchMetar.Core.Features.DataWarehouse.Features.Notification.Options;

public class KnmiMqttOptions
{
    public required string Host { get; set; }
    
    public required string Token { get; set; }
    
    public required string ClientId { get; set; }
}