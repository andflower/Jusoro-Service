using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace JusoroService
{
    public class Program
    {
        [SupportedOSPlatform("windows")]
        public static void Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddWindowsService(option =>
            {
                option.ServiceName = "Jusoro Service";
            });

            LoggerProviderOptions.RegisterProviderOptions<
                EventLogSettings, EventLogLoggerProvider>(builder.Services);

            builder.Services.AddHostedService<JusoroBackgroundService>();

            IHost host = builder.Build();

            /*IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<JusoroBackgroundService>();
                })
                .Build();
*/
            host.Run();
        }
    }
}