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
using UtilityClass = BaseClass.MethodNameExtractor.FuncNameExtractor;

namespace ChangeLogConsole
{

    public class Program
    {
        private static LogWriter? logwriter;
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
            bool val = Directory.Exists(configFilePath);

            // Double check
            if (!Directory.Exists(configFilePath) || files.Count() < 0)
            {
                throw new Exception("Either the Config File Path does not exist or there are different Configs in the assigned path.");
            }

            string configFile = files[0];
            string logFilePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "tmp");

            logwriter = new(configFile, logFilePath);
            reader = new(configFile, logwriter);
            jsonHandler = new(logwriter);
            _config.ConfigFilePath = configFile;

            // Setting up and running ChangeLogConsole for Creating/Appending ChangeLog:
            if (_config == null)
            {
                logwriter.LogWrite($@"Error Message: Unable to find the path to save Commit File.", "MainProgram", UtilityClass.GetMethodName(), MessageLevels.Fatal);
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

            string? tarRepo = reader.ReadInfo("Repo", "changelogSettings");
            RepoMode mode;

            if(tarRepo == null)
            {
                logwriter.LogWrite($@"Error Message: Value for the Target Repo is empty.", NameSpace, UtilityClass.GetMethodName(), MessageLevels.Fatal);
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
                    logwriter.LogWrite($@"Error Message: Selected Repo Mode can not be found.", NameSpace, UtilityClass.GetMethodName(), MessageLevels.Fatal);
                    return;
                }
            }

            _repo = APIFactory<DBNull>.GetAPIRepo(mode, _config, jsonHandler, reader, logwriter);
            var clientProvider = new ClientProvider<DBNull>(logwriter, _config);
            clientProvider.clientBase = _config.runType;
            clientProvider.appName = reader.ReadInfo("RepositoryName", "changelogSettings");
            clg = new(_repo, _config, logwriter, _config.logfilepath, clientProvider);

            try
            {
                clg.ChangeLogReaderWriter().Wait(15000000);
            }
            catch (Exception ex)
            {
                logwriter.LogWrite($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}", "MainProgram",UtilityClass.GetMethodName(), MessageLevels.Fatal);
            }
        }
    }
}