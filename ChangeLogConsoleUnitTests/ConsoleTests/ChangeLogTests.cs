using BaseClass.Config;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Classes;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogConsoleUnitTests.ConsoleTests
{
    public class ChangeLogTests
    {
        private LogWriter logwriter;
        private ConfigHandler configReader;
        private string logpath;
        private IAPIRepo _repo;

        [SetUp]
        public void Setup()
        {
            string configpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest.config";
            logpath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";

            if (Directory.Exists(logpath))
            {
                Directory.Delete(logpath, true); // Ensure the log directory is clean before starting the test
            }

            logwriter = new LogWriter(configpath, logpath);

            configReader = new(configpath, logwriter);
            Environment.SetEnvironmentVariable("Test", "Hello_Unit_Test", EnvironmentVariableTarget.Process);
        }

        [Test]
        public void CommitCaptureModeTestSelection()
        {
            var mode = APIFactory.GetAPIRepo(RepoMode.AzureDevOps,
                new CLGConfig(),
                new JSONFileHandler(logwriter),
                configReader,
                logwriter);

            if (mode != null)
            {
                Assert.That(mode.GetType() == typeof(AzureDevOps), "Mode is not Azure DevOps");
            }
            else
            {
                Assert.Fail("Unable to obtain a valid API Repository Mode");
            }
        }
    }
}
