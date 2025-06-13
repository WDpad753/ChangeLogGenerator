using BaseClass.Config;
using BaseLogger;
using Common.Abstractions;

namespace ChangeLogConsoleUnitTests.BaseTests
{
    public class CoreLibTests
    {
        private LogWriter logwriter;
        private string logpath;
        private string ConfigFilePath;
        private string appName = "ConsoleTest";

        [SetUp]
        public void Setup()
        {
            string configpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest.config";
            logpath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";

            ConfigFilePath = configpath;

            if (Directory.Exists(logpath))
            {
                Directory.Delete(logpath, true); // Ensure the log directory is clean before starting the test
            }

            logwriter = new LogWriter(configpath,logpath);
        }

        [Test]
        public void ConfigReaderTest()
        {
            ConfigReader configReader = new(ConfigFilePath,logwriter);

            string? val = configReader.ReadInfo("AppName");

            if (val != null)
            {
                Assert.That(appName == val, "Value is not equal");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Configuration File");
            }
        }
    }
}