using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Pages;

public partial class Airport : ComponentBase
{
    [Parameter]
    public required string AirportIcao { get; set; }
}