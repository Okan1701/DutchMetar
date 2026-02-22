using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Components.PageLayout;

public partial class PageLayout : ComponentBase
{
    [Parameter]
    public required PageStatus Status { get; set; }
    
    [Parameter]
    public required RenderFragment PageContent { get; set; }
}