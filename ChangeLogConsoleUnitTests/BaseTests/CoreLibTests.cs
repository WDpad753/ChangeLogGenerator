using BaseClass.Base;
using BaseClass.Base.Interface;
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
        private IBase? baseConfig;
        private LogWriter logwriter;
        private ConfigHandler configReader;
        private JSONFileHandler jsonFileHandler;
        private EnvFileHandler envFileHandler;
        private EnvHandler envHandler;
        private string logpath;
        private string configPath;
        private static string LaunchJsonConfigFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\launchsettings.json";
        private static string envConfigFilePath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\EnvFileTest.env";
        private static string xmlConfigFilePath1 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\XMLFileTest1.xml";
        private static string xmlConfigFilePath2 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\XMLFileTest2.xml";
        private static string jsonTestFilesPath = @$"{AppDomain.CurrentDomain.BaseDirectory}TestFiles\JSON";

        [SetUp]
        public void Setup()
        {
            string configpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest.config";
            string Mainconfigpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\MainAppTest.config";

            logpath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";
            configPath = configpath;

            if (Directory.Exists(logpath))
            {
                Directory.Delete(logpath, true); // Ensure the log directory is clean before starting the test
            }

            logwriter = new LogWriter(Mainconfigpath, logpath);

            baseConfig = new BaseSettings()
            {
                Logger = logwriter,
                ConfigPath = configpath,
            };

            configReader = new(baseConfig);
            jsonFileHandler = new(baseConfig);
            envFileHandler = new(baseConfig);
            envHandler = new(baseConfig);
            baseConfig.ConfigHandler = configReader;
            baseConfig.JSONFileHandler = jsonFileHandler;
            baseConfig.EnvFileHandler =  envFileHandler;
            baseConfig.EnvHandler = envHandler;

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

            string? res = envHandler.EnvRead("Test", EnvAccessMode.Project);

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

            string? res = envHandler.EnvRead("Test", EnvAccessMode.User);

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

            string? res = envHandler.EnvRead("Test", EnvAccessMode.System);

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

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, LaunchJsonConfigFilePath, "environmentVariables");

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
        public void JsonConfigEnvSaveTest()
        {
            string val = "Hello_Test";

            envHandler.EnvSave("Test", "Hello_Test", EnvAccessMode.File, LaunchJsonConfigFilePath, "environmentVariables");

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, LaunchJsonConfigFilePath, "environmentVariables");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");

                envHandler.EnvSave("Test", "Hello_Unit_Test", EnvAccessMode.File, LaunchJsonConfigFilePath, "environmentVariables");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }

        [Test]
        public void JsonSearchTest()
        {
            string val = "Alice Smith";

            string filepath = PathCombine.CombinePath(CombinationType.Folder, jsonTestFilesPath, "JsonFile1.json");

            var JsonOutput = baseConfig.JSONFileHandler.GetJson<object>(filepath);

            if(JsonOutput == null)
            {
                Assert.Fail("Output is null.");
            }

            string json = JsonConvert.SerializeObject(JsonOutput);

            var jsonRes = baseConfig.JSONFileHandler.ValueSearch(json, "manager");

            if (jsonRes != null || jsonRes != default)
            {
                Assert.That(jsonRes.Any(res => res.Value.ToString().Equals(val)));
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

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, envConfigFilePath);

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
        public void envConfigEnvSaveTest()
        {
            string val = "Hello_Test";

            envHandler.EnvSave("Test", "Hello_Test", EnvAccessMode.File, envConfigFilePath);

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, envConfigFilePath);

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");

                envHandler.EnvSave("Test", "Hello_Unit_Test", EnvAccessMode.File, envConfigFilePath);
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

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, xmlConfigFilePath1, "EnvironmentVariables");

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
        public void XmlConfigEnvSaveTest1()
        {
            string val = "Hello_Test";

            envHandler.EnvSave("Test", "Hello_Test", EnvAccessMode.File, xmlConfigFilePath1, "EnvironmentVariables");

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, xmlConfigFilePath1, "EnvironmentVariables");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");

                envHandler.EnvSave("Test", "Hello_Unit_Test", EnvAccessMode.File, xmlConfigFilePath1, "EnvironmentVariables");
            }
            else
            {
                Assert.Fail("Unable to Obtain a Value from Enviroment Variables");
            }
        }

        [Test]
        public void XmlConfigEnvSaveTest2()
        {
            string val = "Hello_Test";

            envHandler.EnvSave("Test", "Hello_Test", EnvAccessMode.File, xmlConfigFilePath2, "EnvironmentVariables");

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, xmlConfigFilePath2, "EnvironmentVariables");

            if (res != null)
            {
                Assert.That(val == res, "Value is not equal after modification");

                envHandler.EnvSave("Test", "Hello_Unit_Test", EnvAccessMode.File, xmlConfigFilePath2, "EnvironmentVariables");
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

            string? res = envHandler.EnvRead("Test", EnvAccessMode.File, xmlConfigFilePath2, "EnvironmentVariables");

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
        
        [Test, Order(50)]
        public void CustomConfigWrite2Test()
        {
            DeleteAdd("loggerSettings", "AppName");

            string val = "NewConsoleTest";

            string configpath2 = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest2.config";

            baseConfig.ConfigPath = configpath2;

            configReader = new(baseConfig);

            configReader.SaveInfo("NewConsoleTest", "AppName", "loggerSettings");

            string? res = configReader.ReadInfo("AppName", "loggerSettings");

            if (res != null)
            {
                configReader.SaveInfo("NewConsoleTest", "AppName", "loggerSettings");
                configReader.SaveInfo("NewConsoleTest", "AppName");
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

                XElement targetNode = xdoc.Descendants(mainKey).FirstOrDefault();
                if (targetNode == null)
                {
                    logwriter.LogWrite($"No element named '{mainKey}' found.",
                        GetType().Name, nameof(DeleteAdd), MessageLevels.Fatal);
                    return;
                }

                XElement container = targetNode.Element("settings") ?? targetNode;

                IEnumerable<XElement> adds;
                if (string.IsNullOrEmpty(keyToDelete))
                {
                    adds = container.Elements("add").ToList();
                }
                else
                {
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