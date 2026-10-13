using Microsoft.Data.Sqlite;
using System.Reflection;
using Workboard.Data;

namespace Workboard.Api
{
    internal static class ApiExtension
    {
        internal static async Task<WebApplicationBuilder> AddApi(this WebApplicationBuilder builder) {
            string? connectionString = builder.Configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new KeyNotFoundException("No ConnectionString provided");
            }
            SqliteConnectionStringBuilder connectionStringBuilder = new(connectionString);
            connectionStringBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, connectionStringBuilder.DataSource);

            ApiContextOptions opt = new()
            {
                ConnectionString = connectionStringBuilder.ToString()
            };
            ApiContext context = new(opt);
            // don't run db init on doc creation
            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                await context.InitializeDatabaseAsync();
            }
            builder.Services.AddSingleton<ApiContext>(context);

            BoardCache boardCache = new(context);
            builder.Services.AddScoped<BoardCache>((_) => boardCache);
            return builder;
        }
    }
}
