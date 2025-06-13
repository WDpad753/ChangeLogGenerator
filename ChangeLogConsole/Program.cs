using BaseClass;
using BaseClass.Config;
using BaseLogger;
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
        //private LogWriter logwriter = new();
        private ConfigReader reader;

        static void Main(string[] args)
        {
            // Get the current directory (where your executable is located):
            string currentDirectory = Directory.GetCurrentDirectory();
            string currentDirectory2 = AppDomain.CurrentDomain.BaseDirectory;

            // Setting up and running ChangeLogConsole for Creating/Appending ChangeLog:
            string? commitmessagespath = ProgramConfig.commitMessagesPath;

            if (commitmessagespath == null)
            {
                //logwriter.LogWrite($@"Error Message: Unable to find the path to save Commit File.", "MainProgram", UtilityClass.GetMethodName(), MessageLevels.Fatal);
                return;
            }

            string commitmessagesfilename = "ChangeLog.txt";
            //string targetcommitjsonpath = @$"{currentDirectory}\JsonFiles\";
            string targetcommitjsonpath = @$"{currentDirectory2}JsonFiles\";
            string targetcommitjsonfile = "PrevMap.json";
            string backuptargetcommitjsonpath = $@"{Directory.GetParent(currentDirectory2).Parent.Parent.Parent.FullName}\JsonFiles\";
            CLGConfig CLG = new CLGConfig();

            Debug.WriteLine($"Old Directory => {currentDirectory}; tested Directory => {currentDirectory2}");
            CLG.commitlogpath = commitmessagespath;
            CLG.commitlogfilename = commitmessagesfilename;
            CLG.jsonpath = targetcommitjsonpath;
            CLG.backupjsonpath = backuptargetcommitjsonpath;
            CLG.jsonfilename = targetcommitjsonfile;
            string commitfilepath = CLG.logfilepath;

            //ChangeLogWrite clg = new(CLG,,);

            try
            {
                //Task.Run(async () => await clg.ChangeLogReaderWriter(commitfilepath));
                //clg.ChangeLogReaderWriter(commitfilepath).Wait(15000000);
            }
            catch (Exception ex)
            {
                //logwriter.LogWrite($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}", "MainProgram",UtilityClass.GetMethodName(), MessageLevels.Fatal);
            }
        }
    }
}
