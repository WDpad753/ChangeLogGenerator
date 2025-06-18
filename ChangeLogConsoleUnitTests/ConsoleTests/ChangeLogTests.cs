using BaseClass.Config;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
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
    public class ChangeLogTests
    {
        private LogWriter logwriter;
        private ConfigHandler configReader;
        private string logpath;
        private IAPIRepo _repo;
        private WebApplicationFactory<TestAPI.Program> _factory = null!;
        private HttpClient _client = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // Initialize the in-memory test server factory
            _factory = new WebApplicationFactory<TestAPI.Program>()
                // Optionally configure the factory, e.g., override configuration:
                .WithWebHostBuilder(builder =>
                {
                    // Example: you could configure environment, services, etc.
                    // builder.UseEnvironment("Testing");
                    // builder.ConfigureServices(services => { ... });
                });
            _client = _factory.CreateClient(); // Creates HttpClient with a random port
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

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

        [Test]
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

        [Test]
        public async Task AzureEndpoint()
        {
            string baseUrl = _client.BaseAddress!.ToString();

            //// Act
            //var response = await _client.GetAsync("/health");

            //// Assert
            //Assert.That(response.IsSuccessStatusCode, Is.True, "Expected /health to return 200 OK");

            //var content = await response.Content.ReadAsStringAsync();

            //logwriter.LogWrite(content.ToString(), this.GetType().Name, UtilityClass.GetMethodName(), MessageLevels.Log);

            //Assert.That(content, Is.Not.Null.And.Not.Empty, "Health endpoint returned empty content");
        }
    }
}
