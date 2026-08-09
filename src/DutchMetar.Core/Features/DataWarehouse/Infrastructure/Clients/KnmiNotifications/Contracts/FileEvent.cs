using System.Text.Json.Serialization;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiNotifications.Contracts;

public class FileEvent
{
    [JsonPropertyName("specversion")]
    public string? SpecVersion { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("time")]
    public string? Time { get; set; }
    
    [JsonPropertyName("datacontenttype")]
    public string? DataContentType { get; set; }
    
    [JsonPropertyName("data")]
    public FileData? Data { get; set; }
    
}