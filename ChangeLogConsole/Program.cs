using BaseClass;
using BaseClass.Config;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using ChangeLogConsole.Writer;
using ChangeLogCoreLibrary.Model;
//using Common.Abstractions;
//using Common.Abstractions.Models;
using System.Diagnostics;
using System.Reflection.Metadata;
using UtilityClass = BaseClass.MethodNameExtractor.FuncNameExtractor;

namespace ChangeLogConsole
{
    public static class ProgramConfig
    {
        public static string? commitMessagesPath { get; set; }
    }

    public class Program
    {
        //private static readonly ILogWriter logwriter;
        //private static readonly IConfigReader reader;
        private static LogWriter? logwriter;
        private static ConfigHandler? reader;
        private static string? NameSpace = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public static CLGConfig _config { get; }

        static void Main(string[] args)
        {
            // Get the current directory (where your executable is located):
            string currentDirectory = Directory.GetCurrentDirectory();
            string currentDirectory2 = AppDomain.CurrentDomain.BaseDirectory;

            string configFilePath = Path.Combine(currentDirectory2, "Config");
            string[] files = Directory.GetFiles(configFilePath);

            // Double check
            if(!Directory.Exists(configFilePath) || files.Count() > 0)
            {
                throw new Exception("Either the Config File Path does not exist or there are different Configs in the assigned path.");
            }

            string configFile = files[0];
            string logFilePath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "tmp");

            logwriter = new(configFile, logFilePath);
            reader = new(configFile, logwriter);

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

            ChangeLogWrite clg = new(_config, mode, logwriter, logFilePath);

            try
            {
                //Task.Run(async () => await clg.ChangeLogReaderWriter(commitfilepath));
                clg.ChangeLogReaderWriter().Wait(15000000);
            }
            catch (Exception ex)
            {
                //logwriter.LogWrite($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}", "MainProgram",UtilityClass.GetMethodName(), MessageLevels.Fatal);
            }
        }
    }
}
