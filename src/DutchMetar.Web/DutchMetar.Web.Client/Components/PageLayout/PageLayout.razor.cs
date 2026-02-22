using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Components.PageLayout;

public partial class PageLayout : ComponentBase
{
    [Parameter] public required PageStatus Status { get; set; } = PageStatus.Displaying;
    
    [Parameter]
    public required RenderFragment PageContent { get; set; }
    
    [Parameter]
    public required RenderFragment TitleContent { get; set; }
    
    [Parameter]
    public RenderFragment? SubtitleContent { get; set; }
}