using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DutchMetar.Core.Infrastructure;

public static class Extensions
{
    public static void AddDistributedCache(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Simplify local developer setup
        if (environment.IsDevelopment())
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<MemoryDistributedCache>();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString(ConnectionStringConstants.Redis);
            });
        }
    }

    public static void AddDutchMetarDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DutchMetarContext>(opt =>
        {
            opt.UseSqlServer(configuration.GetConnectionString(ConnectionStringConstants.DutchMetarMssql));
        });
    }
}