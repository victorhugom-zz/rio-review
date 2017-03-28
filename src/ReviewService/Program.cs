using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

using static System.String;

namespace ReviewService
{
    public class Program
    {
        public static string[] Args;

        public static void Main(string[] args)
        {
            Program.Args = args;

            var builder = new ConfigurationBuilder();
            builder.AddCommandLine(args);
            var config = builder.Build();

            var webhost = new WebHostBuilder()
                        .UseConfiguration(config);

            var serverUrls = config["server.urls"];
            if(!IsNullOrEmpty(serverUrls))
                webhost.UseUrls(serverUrls);

            var host = webhost.UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .Build();

            host.Run();
        }
    }
}
