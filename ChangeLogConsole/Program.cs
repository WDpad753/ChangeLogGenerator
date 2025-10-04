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
using ChangeLogCoreLibrary.Writer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using static System.Net.Mime.MediaTypeNames;

namespace ChangeLogConsole
{
    public class Program
    {
        //private static IBaseProvider? baseConfig;
        //private static ILogger? logwriter;
        //private static ConfigHandler? reader;
        //private static ChangeLogWrite<DBNull> clg;
        //private static JSONFileHandler jsonHandler;
        //private static IBaseSettings basesetting;
        //private static IAPIRepo<DBNull> _repo;
        //public static CLGConfig _config { get; set; }
        public static BaseSettings baseSettings { get; set; }

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
            IHost host = Host.CreateDefaultBuilder(args).CreateConsoleBase<ChangeLog, Worker>().Build();
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
            provider.RegisterSingleton<XmlHandler>();
            provider.RegisterSingleton<EnvFileHandler>();
            provider.RegisterSingleton<ConfigHandler>();
            provider.RegisterSingleton<CLGConfig>();
            //
            await host.RunAsync();




            //baseConfig = new BaseProvider();

            //// Get the current directory (where your executable is located):
            //string currentDirectory = Directory.GetCurrentDirectory();
            //string currentDirectory2 = AppDomain.CurrentDomain.BaseDirectory;

            //string configFilePath = Path.Combine(currentDirectory2, "Config");
            //string[] files = (string[])Directory.GetFiles(configFilePath, "*.config");
            //bool val = Directory.Exists(configFilePath);

            //// Double check
            //if (!Directory.Exists(configFilePath) || files.Count() < 0)
            //{
            //    throw new Exception("Either the Config File Path does not exist or there are different Configs in the assigned path.");
            //}

            //string configFile = files[0];
            //string logFilePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "tmp");


            //baseConfig.RegisterInstance<ILogger>(new Logger(configFile, logFilePath));
            //baseConfig.RegisterInstance<IBaseSettings>(new BaseSettings()
            //{
            //    ConfigPath = configFile,
            //});
            //baseConfig.RegisterSingleton<XmlHandler>();
            //baseConfig.RegisterSingleton<EnvFileHandler>();
            //baseConfig.RegisterSingleton<ConfigHandler>();
            //baseConfig.RegisterSingleton<JSONFileHandler>();
            //baseConfig.RegisterSingleton<CLGConfig>();

            //_config = baseConfig.GetItem<CLGConfig>();
            //reader = baseConfig.GetItem<ConfigHandler>();
            //jsonHandler = baseConfig.GetItem<JSONFileHandler>();
            //basesetting = baseConfig.GetItem<IBaseSettings>();
            //logwriter =  baseConfig.GetItem<ILogger>();


            //_config.ConfigFilePath = configFile;

            //// Setting up and running ChangeLogConsole for Creating/Appending ChangeLog:
            //if (_config == null)
            //{
            //    logwriter.LogError($@"Error Message: Unable to find the path to save Commit File.");
            //    return;
            //}

            //string commitmessagesfilename = "CommitsChangeLog.txt";
            //string targetcommitjsonpath = @$"{currentDirectory2}JsonFiles\";
            //string targetcommitjsonfile = "PrevMap.json";
            //string backuptargetcommitjsonpath = $@"{Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName}\JsonFiles\";

            //string? repoProject = reader.ReadInfo("Project", "changelogSettings");
            //string? repoName = reader.ReadInfo("RepositoryName", "changelogSettings");

            //_config.logfilepath = Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName;
            //_config.jsonpath = targetcommitjsonpath;
            //_config.jsonfilename = targetcommitjsonfile;
            //_config.backupjsonpath = backuptargetcommitjsonpath;
            //_config.logfilename = commitmessagesfilename;
            //basesetting.FilePath = _config.logfilepath;

            //string? tarRepo = reader.ReadInfo("Repo", "changelogSettings");
            //RepoMode mode;

            //if(tarRepo == null)
            //{
            //    logwriter.LogError($@"Error Message: Value for the Target Repo is empty.");
            //    return;
            //}
            //else
            //{
            //    if(tarRepo.ToLower().Equals("GitHub".ToLower()))
            //    {
            //        mode = RepoMode.GitHub;
            //        _config.runType = "GitHub";
            //    }
            //    else if (tarRepo.ToLower().Equals("AzureDevOps".ToLower()))
            //    {
            //        mode = RepoMode.AzureDevOps;
            //        _config.runType = "AzureDevOps";
            //    }
            //    else
            //    {
            //        logwriter.LogError($@"Error Message: Selected Repo Mode can not be found.");
            //        return;
            //    }
            //}

            //_repo = APIFactory<DBNull>.GetAPIRepo(mode, _config, jsonHandler, reader, logwriter);

            //var clientProvider = new ClientProvider<DBNull>(logwriter, _config);
            //clientProvider.clientBase = _config.runType;
            //clientProvider.appName = reader.ReadInfo("RepositoryName", "changelogSettings");

            //clg = new(_repo, _config, baseConfig, clientProvider);

            //try
            //{
            //    clg.ChangeLogReaderWriter().Wait(15000000);
            //}
            //catch (Exception ex)
            //{
            //    logwriter.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
            //}
        }
    }
}