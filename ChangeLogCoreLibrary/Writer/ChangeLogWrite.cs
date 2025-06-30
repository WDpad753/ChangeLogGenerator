using BaseClass.API;
using BaseClass.API.Interface;
using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Classes;
using ChangeLogCoreLibrary.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.Writer
{
    public class ChangeLogWrite<T> where T : class
    {
        private CLGConfig _config;
        private JSONFileHandler _fileHandler;
        private APIClient<T> _client;
        private HttpClient _testClient;
        private ConfigHandler _reader;
        private LogWriter _logger;
        private PathCombine _pathCombiner;
        private static MapAzureJson prevMapAzureJson = new MapAzureJson();
        private static List<MapGitHubJson> prevMapGithubJson = new List<MapGitHubJson>();
        private readonly IAPIRepo<T> _repo;
        private string _logFilePath;
        //private readonly IWebFactoryProvider? _factoryProvider;
        private readonly ClientProvider<T>? _factoryProvider;

        public ChangeLogWrite(IAPIRepo<T> repo, CLGConfig config, LogWriter Logger, string logFilePath, ClientProvider<T>? clientFactory = null)
        //public ChangeLogWrite(IAPIRepo repo, CLGConfig config, LogWriter Logger, string logFilePath, IWebFactoryProvider? clientFactory = null)
        {
            _config = config;
            _logger = Logger;
            _fileHandler = new(Logger);
            _reader = new(config.ConfigFilePath, Logger);
            _repo = repo;
            _logFilePath = logFilePath;
            _pathCombiner = new(Logger);
            _client = new(Logger, clientFactory);
        }

        public async Task<string?> ChangeLogReaderWriter()
        {
            string? EnvVar = null;
            string? prevMapJsonHS = null;
            string? organization = null;
            string? project = null;
            string? repositoryName = null;

            if (_config.testClient != null)
            {
                prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS");
                organization = _reader.ReadInfo("Organisation");
                project = _reader.ReadInfo("Project");
                repositoryName = _reader.ReadInfo("RepositoryName");
            }
            else
            {
                prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS", "changelogSettings");
                organization = _reader.ReadInfo("Organisation", "changelogSettings");
                project = _reader.ReadInfo("Project", "changelogSettings");
                repositoryName = _reader.ReadInfo("RepositoryName", "changelogSettings");
            }

            //// Base URL for Azure DevOps REST API
            //string baseUrl = $"https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryName}/commits";

            if (_repo.GetType() == typeof(AzureDevOps<T>))
            {
                EnvVar = Environment.GetEnvironmentVariable("Azure_PAT");

                //_client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, APIRepoPath.AzureDevOps, organization, project, "_apis/git/repositories", repositoryName, "commits"),EnvVar,60);

                _client.APIURL = _pathCombiner.CombinePath(CombinationType.URL, APIRepoPath.AzureDevOps, organization, project, "_apis/git/repositories", repositoryName, "commits");
                _client.PerAccTok = EnvVar;
                _client.timeOut = 60;

                //MapAzureJson mapJson = await _client.Get<MapAzureJson>();
                MapAzureJson mapJson = await _client.Get<MapAzureJson>();
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

                    if (!mapJsonHS.Equals(prevMapJsonHS))
                    {
                        _repo.MapJsonReader(mapJson, prevMapAzureJson, mapJsonHS, _logFilePath);
                    }
                    else
                    {
                        Console.WriteLine("No Changes in the Commit history data");
                        return null;
                    }
                }
                else
                {
                    throw new Exception("MapJson is empty");
                }
            }
            else if (_repo.GetType() == typeof(GitHub<T>))
            {
                EnvVar = Environment.GetEnvironmentVariable("GitHub_PAT");

                //_client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, APIRepoPath.Github, organization, project, "_apis/git/repositories", repositoryName, "commits"), EnvVar, 60);

                _client.APIURL = _pathCombiner.CombinePath(CombinationType.URL, APIRepoPath.Github, "repos", organization, repositoryName, "commits").TrimEnd('/');
                //_client.APIURL = _client.APIURL.TrimEnd('/');
                _client.PerAccTok = EnvVar;
                _client.timeOut = 60;

                //MapGitHubJson mapJson = await _client.Get<MapGitHubJson>();
                //string mapJsonHS = Crc32.CalculateHash<MapGitHubJson>(mapJson);

                List<MapGitHubJson> mapJson = await _client.Get<List<MapGitHubJson>>();
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

                    if (!mapJsonHS.Equals(prevMapJsonHS))
                    {
                        _repo.MapJsonReader(mapJson, prevMapGithubJson, mapJsonHS, _logFilePath, _client, EnvVar);
                    }
                    else
                    {
                        Console.WriteLine("No Changes in the Commit history data");
                        return null;
                    }
                }
                else
                {
                    throw new Exception("MapJson is empty");
                }
            }
            else if (_repo.GetType() == typeof(APITest<T>))
            {
                object? mapJson = null;
                object? prevMapGithubJson = null;
                string mapJsonHS = "";
                object? prevMapJson = "";
                string? testAdd = _config.testClient.BaseAddress.ToString();

                if (_config.runType == "AzureDevOps")
                {
                    //_config.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "azure", organization, project, "_apis/git/repositories", repositoryName, "commits"));
                    //_testClient = _config.testClient;
                    //_client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, testAdd, organization, project, "_apis/git/repositories", repositoryName, "commits"), null, 60, _testClient);

                    _client.APIURL = _pathCombiner.CombinePath(CombinationType.URL, testAdd, organization, project, "_apis/git/repositories", repositoryName, "commits");
                    _client.PerAccTok = EnvVar;
                    _client.timeOut = 60;
                    _config.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "azure", organization, project, "_apis/git/repositories", repositoryName, "commits"));
                    _testClient = _config.testClient;
                    //_client.testClient = true;

                    //mapJson = await _client.Get<MapAzureJson>();
                    mapJson = await _client.Get<MapAzureJson>();
                    mapJsonHS = Crc32.CalculateHash<MapAzureJson>(mapJson as MapAzureJson);
                }
                else if (_config.runType == "GitHub")
                {
                    ///repos/{ owner}/{ repo}/commits
                    //_config.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd,"repos", organization, repositoryName, "commits"));
                    //_testClient = _config.testClient;
                    //_client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "repos", organization, repositoryName, "commits"), null, 60, _testClient);

                    _client.APIURL = _pathCombiner.CombinePath(CombinationType.URL, testAdd, "repos", organization, repositoryName, "commits");
                    _client.PerAccTok = EnvVar;
                    _client.timeOut = 60;
                    _config.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "repos", organization, repositoryName, "commits"));
                    _testClient = _config.testClient;
                    //_client.testClient = true;

                    //mapJson = await _client.Get<List<MapGitHubJson>>();
                    mapJson = await _client.Get<List<MapGitHubJson>>();
                    mapJsonHS = Crc32.CalculateHash<List<MapGitHubJson>>(mapJson as List<MapGitHubJson>);
                }

                if (mapJson != null)
                {
                    if (File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename)))
                    {
                        if (_config.runType == "AzureDevOps")
                        {
                            //_client = new APIClient(PathCombine.CombinePath(CombinationType.URL, APIRepoPath.APITest, organization, project, "_apis/git/repositories", repositoryName, "commits"), null, 60);
                            //mapJson = await _client.Get<MapAzureJson>();
                            mapJsonHS = Crc32.CalculateHash<MapAzureJson>(mapJson as MapAzureJson);
                            prevMapJson = _fileHandler.GetJson<MapAzureJson>(Path.Combine(_config.jsonpath, _config.jsonfilename));

                            if (prevMapJson == null)
                            {
                                prevMapJson = _fileHandler.GetJson<MapAzureJson>(Path.Combine(_config.backupjsonpath, _config.jsonfilename));
                            }
                        }
                        else if (_config.runType == "GitHub")
                        {
                            //_client = new APIClient(PathCombine.CombinePath(CombinationType.URL, APIRepoPath.APITest, organization, project, "_apis/git/repositories", repositoryName, "commits"), null, 60);
                            //mapJson = await _client.Get<MapGitHubJson>();
                            mapJsonHS = Crc32.CalculateHash<List<MapGitHubJson>>(mapJson as List<MapGitHubJson>);
                            prevMapJson = _fileHandler.GetJson<List<MapGitHubJson>>(Path.Combine(_config.jsonpath, _config.jsonfilename));

                            if (prevMapJson == null)
                            {
                                prevMapJson = _fileHandler.GetJson<List<MapGitHubJson>>(Path.Combine(_config.backupjsonpath, _config.jsonfilename));
                            }
                        }
                    }

                    if (!mapJsonHS.Equals(prevMapJsonHS))
                    {
                        _repo.MapJsonReader(mapJson, prevMapGithubJson, mapJsonHS, _logFilePath, _client, EnvVar);
                    }
                    else
                    {
                        Console.WriteLine("No Changes in the Commit history data");
                        return null;
                    }
                }
                else
                {
                    throw new Exception("MapJson is empty");
                }
            }

            //return sb.ToString();
            return null;
        }
    }
}
