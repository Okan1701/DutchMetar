using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Shared.Components.Card;

public partial class Card : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public bool Stretch { get; set; }
}