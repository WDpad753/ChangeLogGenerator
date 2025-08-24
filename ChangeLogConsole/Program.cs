using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Client;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Model;
using ChangeLogCoreLibrary.Writer;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;
using BaseClass.Base.Interface;
using BaseClass.Base;

namespace ChangeLogConsole
{

    public class Program
    {
        private static IBase? baseConfig;
        private static ILogger? logwriter;
        private static ConfigHandler? reader;
        private static string? NameSpace = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        private static ChangeLogWrite<DBNull> clg;
        private static JSONFileHandler jsonHandler;
        private static IAPIRepo<DBNull> _repo;
        public static CLGConfig _config { get; set; }

        static void Main(string[] args)
        {
            _config = new();

            // Get the current directory (where your executable is located):
            string currentDirectory = Directory.GetCurrentDirectory();
            string currentDirectory2 = AppDomain.CurrentDomain.BaseDirectory;

            string configFilePath = Path.Combine(currentDirectory2, "Config");
            string[] files = (string[])Directory.GetFiles(configFilePath, "*.config");
            //string[] files = (string[])Directory.GetFiles(configFilePath);
            bool val = Directory.Exists(configFilePath);

            // Double check
            if (!Directory.Exists(configFilePath) || files.Count() < 0)
            {
                throw new Exception("Either the Config File Path does not exist or there are different Configs in the assigned path.");
            }

            string configFile = files[0];
            string logFilePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "tmp");

            logwriter = new Logger(configFile, logFilePath);
            //reader = new(configFile, logwriter);

            baseConfig = new BaseSettings()
            {
                Logger = logwriter,
                ConfigPath = configFile,
            };

            reader = new(baseConfig);
            //jsonHandler = new(logwriter);
            jsonHandler = new(baseConfig);

            baseConfig.ConfigHandler = reader;
            baseConfig.JSONFileHandler = jsonHandler;

            _config.ConfigFilePath = configFile;

            // Setting up and running ChangeLogConsole for Creating/Appending ChangeLog:
            if (_config == null)
            {
                logwriter.LogError($@"Error Message: Unable to find the path to save Commit File.");
                return;
            }

            string commitmessagesfilename = "ChangeLog.txt";
            //string targetcommitjsonpath = @$"{currentDirectory}\JsonFiles\";
            string targetcommitjsonpath = @$"{currentDirectory2}JsonFiles\";
            string targetcommitjsonfile = "PrevMap.json";
            string backuptargetcommitjsonpath = $@"{Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName}\JsonFiles\";

            string? repoProject = reader.ReadInfo("Project", "changelogSettings");
            string? repoName = reader.ReadInfo("RepositoryName", "changelogSettings");
            //_config.logfilepath = PathCombine.CombinePath(CombinationType.Folder, Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName);
            _config.logfilepath = Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName;
            _config.jsonpath = targetcommitjsonpath;
            _config.jsonfilename = targetcommitjsonfile;
            _config.backupjsonpath = backuptargetcommitjsonpath;
            _config.logfilename = commitmessagesfilename;
            baseConfig.FilePath = _config.logfilepath;

            string? tarRepo = reader.ReadInfo("Repo", "changelogSettings");
            RepoMode mode;

            if(tarRepo == null)
            {
                logwriter.LogError($@"Error Message: Value for the Target Repo is empty.");
                return;
            }
            else
            {
                if(tarRepo.ToLower().Equals("GitHub".ToLower()))
                {
                    mode = RepoMode.GitHub;
                    _config.runType = "GitHub";
                }
                else if (tarRepo.ToLower().Equals("AzureDevOps".ToLower()))
                {
                    mode = RepoMode.AzureDevOps;
                    _config.runType = "AzureDevOps";
                }
                else
                {
                    logwriter.LogError($@"Error Message: Selected Repo Mode can not be found.");
                    return;
                }
            }

            //_repo = APIFactory<DBNull>.GetAPIRepo(mode, _config, jsonHandler, reader, logwriter);
            _repo = APIFactory<DBNull>.GetAPIRepo(mode,_config,baseConfig);
            //var clientProvider = new ClientProvider<DBNull>(logwriter, _config);
            var clientProvider = new ClientProvider<DBNull>(baseConfig, _config);
            clientProvider.clientBase = _config.runType;
            clientProvider.appName = reader.ReadInfo("RepositoryName", "changelogSettings");
            //clg = new(_repo, _config, logwriter, _config.logfilepath, clientProvider);
            clg = new(_repo, _config, baseConfig, clientProvider);

            try
            {
                clg.ChangeLogReaderWriter().Wait(15000000);
            }
            catch (Exception ex)
            {
                logwriter.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
            }
        }
    }
}