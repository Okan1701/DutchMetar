using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DutchMetar.Web.Client.Components.Card;

public partial class CardBigValue : ComponentBase
{
    [Parameter]
    public required string Value { get; set; }
    
    [Parameter]
    public required string Label { get; set; }
    
    [Parameter]
    public Icon? CardIcon { get; set; }
}