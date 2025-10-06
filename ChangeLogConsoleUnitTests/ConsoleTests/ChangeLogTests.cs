using BaseClass.API.Interface;
using BaseClass.Base;
using BaseClass.Base.Interface;
using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using ChangeLogCoreLibrary.APIRepositories.Client;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Classes;
using ChangeLogCoreLibrary.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

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
                .WithWebHostBuilder(builder =>{
                    //builder.UseEnvironment("Testing");
                });
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            Factory.Dispose();
        }
    }

    public class ChangeLogWrite<T> where T : class
    {
        private readonly IBaseProvider? _provider;
        private CLGConfig? _config;
        private JSONFileHandler? _fileHandler;
        public APIClient<T>? _testclient { get; set; }
        private ConfigHandler? _reader;
        private EnvHandler? _envreader;
        private ILogger? _logger;
        private static MapAzureJson prevMapAzureJson = new MapAzureJson();
        private static List<MapGitHubJson> prevMapGithubJson = new List<MapGitHubJson>();
        private readonly IAPIRepo<T>? _repo;
        private string? _logFilePath;
        private ClientProvider<T>? _factoryProvider;
        private bool _disposed;

        public ChangeLogWrite(IBaseProvider provider)
        {
            _provider = provider;

            _logger = provider.GetItem<ILogger>();
            _reader = provider.GetItem<ConfigHandler>();
            _config = provider.GetItem<CLGConfig>();
            _factoryProvider = provider.GetItem<ClientProvider<T>>();
            _testclient = provider.GetItem<APIClient<T>>();
            _fileHandler = provider.GetItem<JSONFileHandler>();
            _repo = provider.GetItem<IAPIRepo<T>>();
            _envreader = provider.GetItem<EnvHandler>();
            var baseSettings = provider.GetItem<IBaseSettings>();

            _factoryProvider.clientBase = _config.runType;
            _factoryProvider.appName = _reader?.ReadInfo("RepositoryName", "changelogSettings");
            _logFilePath = baseSettings.FilePath;
        }

        public async Task ChangeLogReaderWriter()
        {
            string? EnvVar = null;
            string? Envvar = null;
            string? prevMapJsonHS = null;
            string? organization = null;
            string? project = null;
            string? repositoryName = null;

            prevMapJsonHS = _reader?.ReadInfo("PrevMapJSONHS", "changelogSettings");
            organization = _reader?.ReadInfo("Organisation", "changelogSettings");
            project = _reader?.ReadInfo("Project", "changelogSettings");
            repositoryName = _reader?.ReadInfo("RepositoryName", "changelogSettings");
            Envvar = _reader?.ReadInfo("PAT", "changelogSettings");

            _config.Organisation = organization;
            _config.Project = project;
            _config.RepositoryName = repositoryName;

            try
            {
                if (_repo.GetType() == typeof(AzureDevOps<T>))
                {
                    EnvVar = Envvar == "" ? null : Environment.GetEnvironmentVariable(Envvar);
                    _testclient.PerAccTok = EnvVar;
                    _testclient.timeOut = 60;
                    _factoryProvider.clientBase = "AzureDevOps";

                    MapAzureJson mapJson = await _testclient.Get<MapAzureJson>();
                    string mapJsonHS = Crc32.CalculateHash<MapAzureJson>(mapJson);

                    if (mapJson != null)
                    {
                        if (File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename)))
                        {
                            prevMapAzureJson = _fileHandler.GetJson<MapAzureJson>(Path.Combine(_config.jsonpath, _config.jsonfilename));
                            if (prevMapAzureJson == null)
                            {
                                prevMapAzureJson = _fileHandler.GetJson<MapAzureJson>(Path.Combine(_config.backupjsonpath, _config.jsonfilename));
                            }
                        }

                        if (!mapJsonHS.Equals(prevMapJsonHS) || !File.Exists(PathCombine.CombinePath(CombinationType.Folder, _logFilePath, _config.logfilename)))
                        {
                            _repo.MapJsonReader(mapJson, prevMapAzureJson, mapJsonHS, _logFilePath);
                        }
                        else
                        {
                            Console.WriteLine("No Changes in the Commit history data");
                            return;
                        }
                    }
                    else
                    {
                        throw new Exception("MapJson is empty");
                    }
                }
                else if (_repo.GetType() == typeof(GitHub<T>))
                {
                    EnvVar = Envvar == "" ? null : _envreader.EnvRead(Envvar, EnvAccessMode.User);
                    _testclient.PerAccTok = EnvVar;
                    _testclient.timeOut = 60;
                    _factoryProvider.clientBase = "GitHub";

                    List<MapGitHubJson> mapJson = await _testclient.Get<List<MapGitHubJson>>();
                    string mapJsonHS = Crc32.CalculateHash<List<MapGitHubJson>>(mapJson as List<MapGitHubJson>);

                    if (mapJson != null)
                    {
                        if (File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename)))
                        {
                            prevMapGithubJson = _fileHandler.GetJson<List<MapGitHubJson>>(Path.Combine(_config.jsonpath, _config.jsonfilename));
                            if (prevMapGithubJson == null)
                            {
                                prevMapGithubJson = _fileHandler.GetJson<List<MapGitHubJson>>(Path.Combine(_config.backupjsonpath, _config.jsonfilename));
                            }
                        }

                        if (!mapJsonHS.Equals(prevMapJsonHS) || !File.Exists(PathCombine.CombinePath(CombinationType.Folder, _logFilePath, _config.logfilename)))
                        {
                            _repo.MapJsonReader(mapJson, prevMapGithubJson, mapJsonHS, _logFilePath, _testclient, EnvVar);
                        }
                        else
                        {
                            Console.WriteLine("No Changes in the Commit history data");
                            return;
                        }
                    }
                    else
                    {
                        throw new Exception("MapJson is empty");
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
                return;
            }
        }


    }

    [TestFixture]
    public class ChangeLogTests
    {
        public IBaseProvider provider { get; private set; }

        private IBaseSettings? baseConfig;
        private ILogger? logwriter;
        private ConfigHandler configReader;
        private string logpath;
        private IAPIRepo<TestAPI.Program> _repo;
        private IAPIRepo<DBNull> _repoFinal;
        private HttpClient _client = null;
        private CLGConfig _config;
        private JSONFileHandler _jsonFileHandler;
        private string logFilePath;
        private string azureJsonfile = "AppAzureJson.json";
        private string githubJsonfile = "AppGithubJson.json";
        private string finalconfigpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest3.config";


        [OneTimeSetUp]
        public void GlobalTestsSetup()
        {
            provider = new BaseProvider();
            _client = TestEnvironment.Factory.CreateClient();
        }

        [SetUp]
        public void Setup()
        {
            _client = TestEnvironment.Factory.CreateClient();

            //string configpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config\AppTest.config";
            string configpath = finalconfigpath;
            logpath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";
            string jsonpath = @$"{AppDomain.CurrentDomain.BaseDirectory}JsonFiles\";
            string logfilepath = @$"{AppDomain.CurrentDomain.BaseDirectory}TempLogs\";
            string azurelogfilepath = @$"{AppDomain.CurrentDomain.BaseDirectory}AzureChangeLog.txt";
            string gitlogfilepath = @$"{AppDomain.CurrentDomain.BaseDirectory}GitHubChangeLog.txt";

            if (Directory.Exists(logpath))
            {
                Directory.Delete(logpath, true);
            }

            if (File.Exists(logfilepath))
            {
                File.Delete(logfilepath);
            }

            if (File.Exists(azurelogfilepath))
            {
                File.Delete(Path.Combine(jsonpath, azureJsonfile));
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AzureChangeLog.txt"));
            }

            if (File.Exists(gitlogfilepath))
            {
                File.Delete(Path.Combine(jsonpath, githubJsonfile));
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GitHubChangeLog.txt"));
            }


            provider.RegisterInstance<IBaseSettings>(new BaseSettings
            {
                ConfigPath = configpath,
            });
            provider.RegisterInstance<ILogger>(new Logger(configpath, logpath));
            provider.RegisterInstance<ValueCollector<string>>(new ValueCollector<string>("CommitChangeLog", "ConsoleName"));
            provider.RegisterInstance<ValueCollector<DatabaseMode>>(new ValueCollector<DatabaseMode>(DatabaseMode.None, "DatabaseMode"));
            provider.RegisterSingleton<EnvHandler>();
            provider.RegisterSingleton<XmlHandler>();
            provider.RegisterSingleton<EnvFileHandler>();
            provider.RegisterSingleton<ConfigHandler>();
            provider.RegisterSingleton<JSONFileHandler>();
            provider.RegisterSingleton<CLGConfig>();


            logwriter = provider.GetItem<ILogger>();
            baseConfig = provider.GetItem<IBaseSettings>();
            configReader = provider.GetItem<ConfigHandler>();
            _config = provider.GetItem<CLGConfig>();
            _jsonFileHandler = provider.GetItem<JSONFileHandler>();

            provider.RegisterInstance<IAPIRepo<TestAPI.Program>>(APIFactory<TestAPI.Program>.GetAPIRepo(RepoMode.AzureDevOps,_config,_jsonFileHandler,configReader,logwriter));


            configReader.SaveInfo("", "PrevMapJSONHS", "changelogSettings");

            _config.ConfigFilePath = configpath;
            _config.logfilepath = logfilepath;
            _config.runType = "AzureDevOps";
            _config.jsonpath = jsonpath;
            //_config.testClient = _client;
            _config.backupjsonpath = jsonpath;

            logFilePath = Path.GetFileName(Path.GetDirectoryName(logfilepath));

            string projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;
            string projectRoot2 = Directory.GetCurrentDirectory();
            Environment.SetEnvironmentVariable("Test", "Hello_Unit_Test", EnvironmentVariableTarget.Process);
        }

        [Test]
        public void CommitCaptureModeTestSelection()
        {
            var mode = provider.GetItem<IAPIRepo<TestAPI.Program>>();

            if (mode != null)
            {
                Assert.That(mode.GetType() == typeof(AzureDevOps<TestAPI.Program>), "Mode is not Azure DevOps");
            }
            else
            {
                Assert.Fail("Unable to obtain a valid API Repository Mode");
            }
        }

        [Test, Order(1)]
        public async Task HealthEndpoint_Returns200()
        {
            var response = await _client.GetAsync("/health");

            Assert.That(response.IsSuccessStatusCode, Is.True, "Expected /health to return 200 OK");

            var content = await response.Content.ReadAsStringAsync();

            logwriter.LogBase(content.ToString());

            Assert.That(content, Is.Not.Null.And.Not.Empty, "Health endpoint returned empty content");

            _client.Dispose();
        }

        [Test, Order(2)]
        public async Task AzureEndpoint()
        {
            _config.ConfigFilePath = finalconfigpath;
            baseConfig.ConfigPath = finalconfigpath;

            string baseUrl = _client.BaseAddress!.ToString();

            provider.RegisterInstance<ClientProvider<TestAPI.Program>>(new ClientProvider<TestAPI.Program>(logwriter, baseConfig, _config, TestEnvironment.Factory));

            var clientProvider = provider.GetItem<ClientProvider<TestAPI.Program>>();
            provider.RegisterInstance<APIClient<TestAPI.Program>>(new(logwriter, clientProvider));

            if (baseUrl == null || baseUrl == string.Empty)
            {
                Assert.Fail("Base URL is not set or is empty.");
            }
            else
            {
                logwriter.LogBase(baseUrl);
            }

            _config.jsonfilename = azureJsonfile;
            _config.logfilename = "ChangeLog.txt";
            _config.runType = "AzureDevOps";
            baseConfig.FilePath = _config.logfilepath;

            _repo = provider.GetItem<IAPIRepo<TestAPI.Program>>();

            if (_repo == null)
            {
                Assert.Fail("Unable to obtain a valid API Repository Mode");
            }

            ChangeLogWrite<TestAPI.Program> clg = new ChangeLogWrite<TestAPI.Program>(provider);

            try
            {
                clg.ChangeLogReaderWriter().Wait(15000000);
            }
            catch (Exception ex)
            {
                logwriter.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
            }

            bool ChangeLogExists = File.Exists(Path.Combine(_config.logfilepath, _config.logfilename));
            bool PrevJsonExists = File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename));
            string? PrevJsonHS = configReader.ReadInfo("PrevMapJSONHS", "changelogSettings");
            bool PrevJsonHSExists = PrevJsonHS != null ? true : false;

            if (ChangeLogExists == true && PrevJsonExists == true && PrevJsonHSExists == true)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }

            _client.Dispose();
        }

        [Test, Order(3)]
        public async Task GitHubEndpoint()
        {
            APIClient<TestAPI.Program>.ResetClient();

            _config.ConfigFilePath = finalconfigpath;
            _config.runType = "GitHub";
            baseConfig.ConfigPath = finalconfigpath;

            string baseUrl = _client.BaseAddress!.ToString();

            provider.RegisterInstance<ClientProvider<TestAPI.Program>>(new ClientProvider<TestAPI.Program>(logwriter, baseConfig, _config, TestEnvironment.Factory));

            var clientProvider = provider.GetItem<ClientProvider<TestAPI.Program>>();
            provider.RegisterInstance<APIClient<TestAPI.Program>>(new(logwriter, clientProvider));



            if (baseUrl == null || baseUrl == string.Empty)
            {
                Assert.Fail("Base URL is not set or is empty.");
            }
            else
            {
                logwriter.LogBase(baseUrl);
            }

            _config.jsonfilename = githubJsonfile;
            _config.logfilename = "ChangeLog.txt";
            baseConfig.FilePath = _config.logfilepath;

            provider.RegisterInstance<IAPIRepo<TestAPI.Program>>(APIFactory<TestAPI.Program>.GetAPIRepo(RepoMode.GitHub, _config, _jsonFileHandler, configReader, logwriter));

            _repo = provider.GetItem<IAPIRepo<TestAPI.Program>>();

            if (_repo == null)
            {
                Assert.Fail("Unable to obtain a valid API Repository Mode");
            }

            ChangeLogWrite<TestAPI.Program> clg = new ChangeLogWrite<TestAPI.Program>(provider);

            try
            {
                clg.ChangeLogReaderWriter().Wait(15000000);
            }
            catch (Exception ex)
            {
                logwriter.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
            }

            bool ChangeLogExists = File.Exists(Path.Combine(_config.logfilepath, _config.logfilename));
            bool PrevJsonExists = File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename));
            string? PrevJsonHS = configReader.ReadInfo("PrevMapJSONHS", "changelogSettings");
            bool PrevJsonHSExists = PrevJsonHS != null && PrevJsonHS != "" ? true : false;

            if (ChangeLogExists == true && PrevJsonExists == true && PrevJsonHSExists == true)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            _client?.Dispose();
            provider?.Dispose();
        }

        [TearDown]
        public void Teardown()
        {
            _client?.Dispose();
        }
    }
}
