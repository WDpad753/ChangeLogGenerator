using BaseClass.Config;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using ChangeLogConsole.Writer;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Classes;
using ChangeLogCoreLibrary.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UtilityClass = BaseClass.MethodNameExtractor.FuncNameExtractor;


namespace ChangeLogConsoleUnitTests.ConsoleTests
{
    [SetUpFixture]
    public class TestEnvironment
    {
        public static WebApplicationFactory<TestAPI.Program> Factory { get; private set; }

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Factory = new WebApplicationFactory<TestAPI.Program>()
                .WithWebHostBuilder(builder =>
                {
                    // builder.UseEnvironment("Testing");
                });
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            Factory.Dispose();
        }
    }

    [TestFixture]
    //public class ChangeLogTests : ApiTestBase
    public class ChangeLogTests
    {
        private LogWriter logwriter;
        private ConfigHandler configReader;
        private string logpath;
        private IAPIRepo _repo;
        //private WebApplicationFactory<TestAPI.Program> _factory = null!;
        private HttpClient _client = null;
        private CLGConfig _config;
        private JSONFileHandler _jsonFileHandler;
        private string logFilePath;

        [SetUp]
        public void Setup()
        {
            _client = TestEnvironment.Factory.CreateClient();

            string configpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest.config";
            logpath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";
            string jsonpath = @$"{AppDomain.CurrentDomain.BaseDirectory}JsonFiles\";
            string jsonfile = "AppJson.json";
            string logfilepath = @$"{AppDomain.CurrentDomain.BaseDirectory}ChangeLog.txt";

            if (Directory.Exists(logpath))
            {
                Directory.Delete(logpath, true); // Ensure the log directory is clean before starting the test
            }

            if(File.Exists(logfilepath))
            {
                File.Delete(logfilepath);
            }

            logwriter = new LogWriter(configpath, logpath);

            configReader = new(configpath, logwriter);
            Environment.SetEnvironmentVariable("Test", "Hello_Unit_Test", EnvironmentVariableTarget.Process);

            _config = new CLGConfig();
            _jsonFileHandler = new JSONFileHandler(logwriter);

            _config.ConfigFilePath = configpath;
            _config.logfilepath = logfilepath;
            _config.runType = "AzureDevOps";
            _config.jsonpath = jsonpath;
            _config.jsonfilename = jsonfile;
            _config.testClient = _client;
            _config.backupjsonpath = jsonpath;

            logFilePath = logfilepath;

            string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            string projectRoot2 = Directory.GetCurrentDirectory();
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

        [Test, Order(1)]
        public async Task HealthEndpoint_Returns200()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.That(response.IsSuccessStatusCode, Is.True, "Expected /health to return 200 OK");
            
            var content = await response.Content.ReadAsStringAsync();

            logwriter.LogWrite(content.ToString(), this.GetType().Name, UtilityClass.GetMethodName(), MessageLevels.Log);

            Assert.That(content, Is.Not.Null.And.Not.Empty, "Health endpoint returned empty content");
        }

        [Test, Order(2)]
        public async Task AzureEndpoint()
        {
            string baseUrl = _client.BaseAddress!.ToString();

            if(baseUrl == null || baseUrl == string.Empty)
            {
                Assert.Fail("Base URL is not set or is empty.");
            }
            else
            {
                logwriter.LogWrite(baseUrl, "", "", MessageLevels.Log);
            }

                _repo = APIFactory.GetAPIRepo(RepoMode.APITest,
                    _config,
                    _jsonFileHandler,
                    configReader,
                    logwriter);

            if (_repo == null)
            {
                Assert.Fail("Unable to obtain a valid API Repository Mode");
            }

            ChangeLogWrite clg = new ChangeLogWrite(_repo, _config, logwriter, logFilePath);

            try
            {
                //Task.Run(async () => await clg.ChangeLogReaderWriter(commitfilepath));
                clg.ChangeLogReaderWriter().Wait(15000000);
            }
            catch (Exception ex)
            {
                //logwriter.LogWrite($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}", "MainProgram",UtilityClass.GetMethodName(), MessageLevels.Fatal);
            }

            bool ChangeLogExists = File.Exists(_config.logfilepath);
            bool PrevJsonExists = File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename));
            string? PrevJsonHS = configReader.ReadInfo("PrevMapJSONHS");
            bool PrevJsonHSExists = PrevJsonHS != null ? true : false;

            if(ChangeLogExists == true && PrevJsonExists == true && PrevJsonHSExists == true)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [TearDown]
        public void Teardown()
        {
            _client?.Dispose();
        }
    }
}
