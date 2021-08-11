using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BandoriBot.Apis
{
    internal class Program
    {
        internal static void Main2(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:4/", "http://*:8443/").UseStartup<Startup>();
                });
    }
}
