using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ReviewService
{
    public class Program
    {
        public static string DbHost { get; private set; }
        public static int DbPort { get; private set; }
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "server.urls", "http://localhost:5060" }
            });
            builder.AddCommandLine(args);
            var config = builder.Build();

            DbHost = config["dbHost"];
            DbPort = int.Parse(config["dbPort"]);

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
