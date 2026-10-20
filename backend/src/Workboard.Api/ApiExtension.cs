using Microsoft.Data.Sqlite;
using System.Reflection;
using Workboard.Data;

namespace Workboard.Api
{
    internal static class ApiExtension
    {
        internal static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
        {
            // skip DI for openapi doc generation
            if (Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider")
            {
                return builder;
            }

            string? connectionString = builder.Configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new KeyNotFoundException("No ConnectionString provided");
            }
            SqliteConnectionStringBuilder connectionStringBuilder = new(connectionString);
            connectionStringBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, connectionStringBuilder.DataSource);

            using ServiceProvider tempProvider = builder.Logging.Services.BuildServiceProvider();
            ILoggerFactory loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            SchemaUpdater updater = new(new(connectionString.ToString()), loggerFactory);
            updater.Update();

            builder.Services.AddSingleton<ApiContext>(sp =>
            {
                ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                ApiContextOptions opt = new()
                {
                    ConnectionString = connectionStringBuilder.ToString(),
                    LoggerFactory = loggerFactory,
                };
                ApiContext context = new(opt);
                return context;
            });


            builder.Services.AddScoped<BoardCache>(sp =>
            {
                ApiContext context = sp.GetRequiredService<ApiContext>();
                BoardCache boardCache = new(context);
                return boardCache;
            });
            return builder;
        }
    }
}
