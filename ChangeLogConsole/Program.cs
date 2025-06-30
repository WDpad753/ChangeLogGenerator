using BaseClass;
using BaseClass.Config;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
//using ChangeLogConsole.Writer;
using ChangeLogCoreLibrary.Model;
using ChangeLogCoreLibrary.Writer;
using System;

//using Common.Abstractions;
//using Common.Abstractions.Models;
using System.Diagnostics;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;
using UtilityClass = BaseClass.MethodNameExtractor.FuncNameExtractor;

namespace ChangeLogConsole
{
    //public class ProgramConfig
    //{
    //    public string? commitMessagesPath { get; set; }
    //    public string? ConfigFilePath { get; set; }
    //}

    public class Program
    {
        //private static readonly ILogWriter logwriter;
        //private static readonly IConfigReader reader;
        private static LogWriter? logwriter;
        private static ConfigHandler? reader;
        private static string? NameSpace = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        //private static ChangeLogWrite<null> clg;
        private static JSONFileHandler jsonHandler;
        private static List<string> ext = new List<string> { "config" };
        public static CLGConfig _config { get; set; }

        static void Main(string[] args)
        {
            _config = new();

            // Get the current directory (where your executable is located):
            string currentDirectory = Directory.GetCurrentDirectory();
            string currentDirectory2 = AppDomain.CurrentDomain.BaseDirectory;

            string configFilePath = Path.Combine(currentDirectory2, "Config");
            //string[] files = Directory.GetFiles(configFilePath).Where(s => s.EndsWith(".config", StringComparison.OrdinalIgnoreCase));
            string[] files = (string[])Directory.GetFiles(configFilePath, "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));
            bool val = Directory.Exists(configFilePath);

            // Double check
            if (!Directory.Exists(configFilePath) || files.Count() < 0)
            {
                throw new Exception("Either the Config File Path does not exist or there are different Configs in the assigned path.");
                //configFilePath = commitMessagesPath;
            }

            string configFile = files[0];
            string logFilePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "tmp");

            logwriter = new(configFile, logFilePath);
            reader = new(configFile, logwriter);
            jsonHandler = new(logwriter);
            _config.ConfigFilePath = configFile;
            //_config.logfilepath = logFilePath;

            // Setting up and running ChangeLogConsole for Creating/Appending ChangeLog:
            //string? commitmessagespath = ProgramConfig.commitMessagesPath;

            //if (commitmessagespath == null)
            if (_config == null)
            {
                logwriter.LogWrite($@"Error Message: Unable to find the path to save Commit File.", "MainProgram", UtilityClass.GetMethodName(), MessageLevels.Fatal);
                return;
            }

            //string commitmessagesfilename = "ChangeLog.txt";
            ////string targetcommitjsonpath = @$"{currentDirectory}\JsonFiles\";
            //string targetcommitjsonpath = @$"{currentDirectory2}JsonFiles\";
            //string targetcommitjsonfile = "PrevMap.json";
            //string backuptargetcommitjsonpath = $@"{Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName}\JsonFiles\";

            //Debug.WriteLine($"Old Directory => {currentDirectory}; tested Directory => {currentDirectory2}");
            //_config.commitlogpath = commitmessagespath;
            //_config.commitlogfilename = commitmessagesfilename;
            //_config.jsonpath = targetcommitjsonpath;
            //CLG.backupjsonpath = backuptargetcommitjsonpath;
            //CLG.jsonfilename = targetcommitjsonfile;
            //string commitfilepath = CLG.logfilepath;

            string? tarRepo = reader.ReadInfo("Repo");
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
                }
                else if (tarRepo.ToLower().Equals("AzureDevOps".ToLower()))
                {
                    mode = RepoMode.AzureDevOps;
                }
                else
                {
                    logwriter.LogWrite($@"Error Message: Selected Repo Mode can not be found.", NameSpace, UtilityClass.GetMethodName(), MessageLevels.Fatal);
                    return;
                }
            }

            //clg = new(_config, mode, logwriter, logFilePath);

            //try
            //{
            //    //Task.Run(async () => await clg.ChangeLogReaderWriter(commitfilepath));
            //    clg.ChangeLogReaderWriter().Wait(15000000);
            //}
            //catch (Exception ex)
            //{
            //    //logwriter.LogWrite($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}", "MainProgram",UtilityClass.GetMethodName(), MessageLevels.Fatal);
            //}
        }
    }
}
