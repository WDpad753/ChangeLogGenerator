using BaseClass.Config;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace ChangeLogConsoleUnitTests.BaseTests
{
    public class CoreLibTests
    {
        private LogWriter logwriter;
        //private LogWriter logwriter2;
        private ConfigHandler configReader;
        //private ConfigHandler configReader2;
        private string logpath;
        private string configPath;
        private static string LaunchJsonConfigFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\launchsettings.json";
        private static string envConfigFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\EnvFileTest.env";
        private static string xmlConfigFilePath1 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\XMLFileTest1.xml";
        private static string xmlConfigFilePath2 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\XMLFileTest2.xml";

        [SetUp]
        public void Setup()
        {
            string configpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest.config";
            //string configpath2 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest2.config";
            logpath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";
            configPath = configpath;

            if (Directory.Exists(logpath))
            {
                Directory.Delete(logpath, true); // Ensure the log directory is clean before starting the test
            }

            logwriter = new LogWriter(configpath, logpath);
            //logwriter2 = new LogWriter(configpath2, logpath);

            configReader = new(configpath, logwriter);
            //configReader2 = new(configpath2, logwriter2);
            Environment.SetEnvironmentVariable("Test", "Hello_Unit_Test", EnvironmentVariableTarget.Process);
        }

        [Test]
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        //[NonParallelizable]
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
        
        
        [Test, Order(1)]
        //[NonParallelizable]
        public void CustomConfigReadTest()
        {
            string val = "ConsoleTest";

            string? res = configReader.ReadInfo("AppName", "loggerSettings");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }
        
        [Test, Order(2)]
        //[NonParallelizable]
        public void CustomConfigWriteTest()
        {
            string val = "NewConsoleTest";

            configReader.SaveInfo("NewConsoleTest", "AppName", "loggerSettings");

            string? res = configReader.ReadInfo("AppName", "loggerSettings");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }
        
        [Test, Order(3)]
        //[NonParallelizable]
        public void CustomConfigWrite2Test()
        {
            DeleteAdd("loggerSettings", "AppName");

            string val = "NewConsoleTest";

            string configpath2 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest2.config";

            configReader = new(configpath2, logwriter);

            configReader.SaveInfo("NewConsoleTest", "AppName", "loggerSettings");

            string? res = configReader.ReadInfo("AppName", "loggerSettings");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }

        private void DeleteAdd(string mainKey, string? keyToDelete = null)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    logwriter.LogWrite($"XML File does not exist in the given path. Path => {configPath}",
                        GetType().Name, nameof(DeleteAdd), MessageLevels.Fatal);
                    return;
                }

                XDocument xdoc = XDocument.Load(configPath);

                // Find the element named mainKey (e.g. "loggerSettings")
                XElement targetNode = xdoc.Descendants(mainKey).FirstOrDefault();
                if (targetNode == null)
                {
                    logwriter.LogWrite($"No element named '{mainKey}' found.",
                        GetType().Name, nameof(DeleteAdd), MessageLevels.Fatal);
                    return;
                }

                // Determine container: if <settings> child exists, use it; otherwise use targetNode
                XElement container = targetNode.Element("settings") ?? targetNode;

                // Find <add> elements to remove:
                IEnumerable<XElement> adds;
                if (string.IsNullOrEmpty(keyToDelete))
                {
                    // remove all <add> under container (direct children)
                    adds = container.Elements("add").ToList();
                }
                else
                {
                    // remove only those whose key attribute matches
                    adds = container.Elements("add")
                        .Where(el => string.Equals(el.Attribute("key")?.Value, keyToDelete, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!adds.Any())
                {
                    logwriter.LogWrite(keyToDelete == null
                        ? $"No <add> elements found under <{container.Name}>."
                        : $"No <add key=\"{keyToDelete}\"/> found under <{container.Name}>.",
                        GetType().Name, nameof(DeleteAdd), MessageLevels.Log);
                    return;
                }

                // Remove them
                foreach (var add in adds)
                    add.Remove();

                xdoc.Save(configPath);
                logwriter.LogWrite($"Removed {adds.Count()} <add> element(s) under <{container.Name}>.", GetType().Name, nameof(DeleteAdd), MessageLevels.Log);
            }
            catch (Exception ex)
            {
                logwriter.LogWrite($"Exception in DeleteAdd: {ex.Message}", GetType().Name, nameof(DeleteAdd), MessageLevels.Fatal);
            }
        }
    }
}