using System.Text.Json.Serialization;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;

/// <summary>
/// <see href="https://tyk-cdn.dataplatform.knmi.nl/notification/index.html#schema-FileEvent"/>
/// </summary>
public class FileData
{
    [JsonPropertyName("datasetName")]
    public string? DataSetName { get; set; }
    
    [JsonPropertyName("datasetVersion")]
    public string? DataSetVersion { get; set; }
    
    [JsonPropertyName("filename")]
    public string? FileName { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}