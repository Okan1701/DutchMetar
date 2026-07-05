namespace DutchMetar.Core.Features.DataWarehouse.Features.Notification.Contracts;

/// <summary>
/// <see href="https://tyk-cdn.dataplatform.knmi.nl/notification/index.html#schema-FileEvent"/>
/// </summary>
public class FileData
{
    public string? DataSetName { get; set; }
    
    public string? DataSetVersion { get; set; }
    
    public string? FileName { get; set; }
    
    public string? Url { get; set; }
}