﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ford.DataContext.Sqlite;

public static class FordContextExtensions
{
    public static IServiceCollection AddFordContext(this IServiceCollection services, string relativePath = "..")
    {
        string databasePath = Path.Combine(relativePath, "Ford.db");
        services.AddDbContext<FordContext>(options =>
            {
                options.UseSqlite($"Data Source={databasePath}");
            }
        );

        return services;
    }
}
