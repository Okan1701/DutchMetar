using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Components.Card;

public partial class Card : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}