using System.IO;
using Account_Removal_Service.Logic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Account_Removal_Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Timers.Timer timer = new System.Timers.Timer { Interval = 30000 };
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            CreateHostBuilder(args).Build().Run();
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            QueuedTasks.ExecuteAction();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((builder) =>
                    {
                        builder.SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.Development.json", true)
                            .AddJsonFile("config/appsettings.Kubernetes.json", true)
                            .AddEnvironmentVariables();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
