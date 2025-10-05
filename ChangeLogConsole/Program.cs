using BaseClass.Base;
using BaseClass.Base.Interface;
using BaseClass.Config;
using BaseClass.ConsoleAppBase.HostBuilderBase;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using ChangeLogCoreLibrary.APIRepositories.Client;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;
using ILogger = BaseLogger.ILogger;

namespace ChangeLogConsole
{
    public class Program
    {
        public static BaseSettings? baseSettings { get; set; }

        static async Task Main(string[] args)
        {
            // Get the current directory (where your executable is located):
            string currentDirectory = Directory.GetCurrentDirectory();
            string currentDirectory2 = AppDomain.CurrentDomain.BaseDirectory;

            string configFilePath = Path.Combine(currentDirectory2, "Config");
            string[] files = (string[])Directory.GetFiles(configFilePath, "*.config");
            bool val = Directory.Exists(configFilePath);

            // Double check
            if (!Directory.Exists(configFilePath) || files.Count() < 0)
            {
                throw new Exception("Either the Config File Path does not exist or there are different Configs in the assigned path.");
            }

            string configFile = files[0];
            string logFilePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "tmp");

            //
            IHost host = Host.CreateDefaultBuilder(args).ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
            }).CreateConsoleBase<ChangeLog, Worker>().Build();
            //
            var provider = host.Services.GetRequiredService<IBaseProvider>();

            baseSettings = new BaseSettings
            {
                ConfigPath = configFile,
            };

            provider.RegisterInstance<IBaseSettings>(baseSettings);
            provider.RegisterInstance<ILogger>(new Logger(configFile, logFilePath));
            provider.RegisterInstance<ValueCollector<string>>(new ValueCollector<string>("CommitChangeLog", "ConsoleName"));
            provider.RegisterInstance<ValueCollector<DatabaseMode>>(new ValueCollector<DatabaseMode>(DatabaseMode.None, "DatabaseMode"));
            provider.RegisterSingleton<EnvHandler>();
            provider.RegisterSingleton<XmlHandler>();
            provider.RegisterSingleton<EnvFileHandler>();
            provider.RegisterSingleton<ConfigHandler>();
            provider.RegisterSingleton<JSONFileHandler>();
            provider.RegisterSingleton<CLGConfig>();
            //
            await host.RunAsync();
        }
    }
}