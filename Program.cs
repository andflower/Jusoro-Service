using System.Security.Principal;

namespace JusoroService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<JusoroBackgroundService>();
                })
                .Build();

            host.Run();
        }
    }
}