using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BandoriBot.Apis
{
    internal class Program
    {
        internal static void Main2(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch
            {

            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:13100/").UseStartup<Startup>();
                });
    }
}
