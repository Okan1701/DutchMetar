using System.Net;
using System.Text.Json.Serialization;

namespace DutchMetar.Web.Server.Models;

public class ApiErrorModel
{
    public HttpStatusCode Status { get; set; }
    
    public required string RequestId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<string>? Errors { get; set; }
}