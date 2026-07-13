namespace DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.Options;

public class KnmiDataSourceOptions
{
    /// <summary>
    /// Authorization token used to authenticate with the KNMI API
    /// </summary>
    public required string AuthorizationToken { get; set; }
    
    /// <summary>
    /// Client ID to be used when connecting to KNMI MQTT.
    /// This value can be anything.
    /// </summary>
    public required string MqttClientId { get; set; }
    
    public required string MqttToken { get; set; }
}