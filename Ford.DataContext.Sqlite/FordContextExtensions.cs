using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ford.DataContext.Sqlite;

public static class FordContextExtensions
{
    public static IServiceCollection AddFordContext(this IServiceCollection services, string relativePath = "..")
    {
        string databasePath = Path.Combine(relativePath, "Northwind.db");
        services.AddDbContext<FordContext>(options =>
            options.UseSqlite($"Data Source={databasePath}")
        );
        return services;
    }
}
