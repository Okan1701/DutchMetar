using Hangfire.Dashboard;

namespace DutchMetar.Hangfire.Host;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow anonymous access for now
        return true;
    }
}