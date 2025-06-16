using BaseClass.Config;
using BaseClass.Model;
using BaseLogger;
using System.Reflection.Metadata;

namespace ChangeLogConsoleUnitTests.BaseTests
{
    public class CoreLibTests
    {
        private LogWriter logwriter;
        private ConfigHandler configReader;
        private string logpath;
        private static string LaunchJsonConfigFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\launchsettings.json";
        private static string envConfigFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\EnvFileTest.env";
        private static string xmlConfigFilePath1 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\XMLFileTest1.xml";
        private static string xmlConfigFilePath2 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\XMLFileTest2.xml";

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
        public void ConfigReaderAssertTest()
        {
            string appName = "ConsoleTest";
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

        [Test]
        public void ConfigReaderModificationTest()
        {
            string? val = configReader.ReadInfo("AppName");

            string newAppName = "NewConsoleTest";
            configReader.SaveInfo(newAppName, "AppName");
            val = configReader.ReadInfo("AppName");

            if (val != null)
            {
                Assert.That(newAppName == val, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Configuration File after modification");
            }
        }

        [Test]
        public void ConfigUserMachineMimicEnvReadTest()
        {
            string val = "Hello_Unit_Test";

            string? res = configReader.EnvRead("Test", EnvAccessMode.Project);

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }

        [Test]
        [Explicit("Only run locally or manually")]
        public void ConfigUserEnvReadTest()
        {
            string val = "Hello_Unit_Test";

            string? res = configReader.EnvRead("Test", EnvAccessMode.User);

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }

        [Test]
        [Explicit("Only run locally or manually")]
        public void ConfigMachineEnvReadTest()
        {
            string val = "Hello_Unit_Test";

            string? res = configReader.EnvRead("Test", EnvAccessMode.System);

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }

        [Test]
        public void JsonConfigEnvReadTest()
        {
            string val = "Hello_Unit_Test";

            string? res = configReader.EnvRead("Test", EnvAccessMode.File, LaunchJsonConfigFilePath, "environmentVariables");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }
        
        [Test]
        public void envConfigEnvReadTest()
        {
            string val = "Hello_Unit_Test";

            string? res = configReader.EnvRead("Test", EnvAccessMode.File, envConfigFilePath);

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }
        
        [Test]
        public void XmlConfigEnvReadTest1()
        {
            string val = "Hello_Unit_Test";

            string? res = configReader.EnvRead("Test", EnvAccessMode.File, xmlConfigFilePath1, "EnvironmentVariables");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }
        
        [Test]
        public void XmlConfigEnvReadTest2()
        {
            string val = "Hello_Unit_Test";

            string? res = configReader.EnvRead("Test", EnvAccessMode.File, xmlConfigFilePath2, "EnvironmentVariables");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }
    }
}