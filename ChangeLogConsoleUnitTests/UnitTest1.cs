using BaseLogger;
using Common.Abstractions;

namespace ChangeLogConsoleUnitTests
{
    public class Tests
    {
        private ILogWriter logwriter;
        private string logpath;

        [SetUp]
        public void Setup()
        {
            string configpath = @"Config\AppTest.config";
            logpath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";

            if(Directory.Exists(logpath))
            {
                Directory.Delete(logpath, true); // Ensure the log directory is clean before starting the test
            }

            logwriter = new LogWriter(configpath,logpath);
        }

        [Test]
        public void Test1()
        {
            if (logwriter == null && Directory.Exists(logpath))
            {
                Assert.Fail("LogWriter is not initialized and Directory Exists.");
            }
            else
            {
                Assert.Pass();
            }
        }
    }
}