using Microsoft.Extensions.Logging;

namespace Workboard.Data
{
    public class ApiContextOptions
    {
        public required string ConnectionString { get; set; }
        public required ILoggerFactory LoggerFactory { get; set; }
    }
}
