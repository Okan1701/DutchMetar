using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DutchMetar.Core.Infrastructure;

public static class Extensions
{
    public static void AddDutchMetarDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DutchMetarContext>(opt =>
        {
            opt.UseSqlServer(configuration.GetConnectionString(DutchMetarContext.ConnectionStringKey));
        });
    }
}