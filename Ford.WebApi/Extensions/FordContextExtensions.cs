using Ford.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Ford.WebApi.Extensions;

public static class FordContextExtensions
{
    public static IServiceCollection AddFordContext(this IServiceCollection services, string relativePath = "..")
    {
        string databasePath = Path.Combine(relativePath, "Ford.db");
        services.AddDbContext<FordContext>(options =>
            {
                options.UseSqlServer();
            }
        );

        return services;
    }
}
