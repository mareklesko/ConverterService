using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using ConverterService.Services;

namespace ConverterService
{
    class Program
    {
        static IConfigurationRoot configuration;
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var isService = !(Debugger.IsAttached || args.Contains("--console"));


            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(options =>
                    {
                        options.SetMinimumLevel(LogLevel.Warning);
                        options.AddConsole();
                        options.AddEventLog(new Microsoft.Extensions.Logging.EventLog.EventLogSettings()
                        {
                            SourceName = "SITE-Service",
                        });
                    });

                    services.AddSingleton(s =>
                    {
                        var log = s.GetRequiredService<ILogger<FileWatcherService>>();
                        return new FileWatcherService(Path.GetFullPath(configuration["FilesPath"]), "*.mp4", log);
                    });

                    services.AddSingleton(s => new FileProcessor(configuration["FFMPegDir"], configuration["OutPath"]));

                    services.AddSingleton<IHostedService>(s =>
                    {
                        var fw = (FileWatcherService)s.GetRequiredService(typeof(FileWatcherService));
                        var fp = s.GetService<FileProcessor>();

                        fw.FileAdded += (file) => fp.ProcessFile(file);
                        return new Service();
                    });

                });

            if (isService)
            {
                await builder.RunAsServiceAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
        static void ConfigureServices(IServiceCollection serviceCollection)
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("config.json", false)
                .Build();

            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
        }
    }
}
