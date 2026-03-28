using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Shared.Components.Card;

public partial class CardBody : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

}