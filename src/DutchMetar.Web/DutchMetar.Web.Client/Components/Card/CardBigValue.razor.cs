using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Components.Card;

public partial class CardBigValue : ComponentBase
{
    [Parameter]
    public required string Value { get; set; }
    
    [Parameter]
    public required string Label { get; set; }
}