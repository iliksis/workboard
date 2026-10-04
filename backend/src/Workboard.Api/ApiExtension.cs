using Microsoft.Data.Sqlite;
using Workboard.Data;

namespace Workboard.Api
{
    internal static class ApiExtension
    {
        private const string fileName = "sqliteSample.db";
        internal static async Task<WebApplicationBuilder> AddApi(this WebApplicationBuilder builder) {
            ApiContext context = new(fileName);
            await context.InitializeDatabaseAsync();
            builder.Services.AddSingleton<ApiContext>(context);

            BoardCache boardCache = new(context);
            builder.Services.AddScoped<BoardCache>((_) => boardCache);
            return builder;
        }
    }
}
