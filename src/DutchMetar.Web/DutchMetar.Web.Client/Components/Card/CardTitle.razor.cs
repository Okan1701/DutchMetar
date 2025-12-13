using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Components.Card;

public partial class CardTitle : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}