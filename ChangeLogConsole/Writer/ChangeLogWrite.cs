using BaseClass.API;
using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Classes;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogConsole.Writer
{
    public class ChangeLogWrite
    {
        //public string configpathfull
        //{
        //    get
        //    {
        //        // Get the current directory (where your executable is located) and set the configpath:
        //        string currentDirectory = Directory.GetCurrentDirectory();
        //        string configfpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config";
        //        string configfilename = @"App.config";
        //        string configpath = Path.Combine(configfpath, configfilename);
        //        return configpath;
        //    }
        //}

        public string configpathfull
        {
            get; set;
            //get
            //{
            //    // Get the current directory (where your executable is located) and set the configpath:
            //    string currentDirectory = Directory.GetCurrentDirectory();
            //    string configfpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config";
            //    string configfilename = @"App.config";
            //    string configpath = Path.Combine(configfpath, configfilename);
            //    return configpath;
            //}
        }

        private CLGConfig _config;
        private JSONFileHandler _fileHandler;
        private APIClient _client;
        private HttpClient _testClient;
        //private readonly IConfigReader _reader;
        private ConfigHandler _reader;
        private LogWriter _logger;
        private PathCombine _pathCombiner;
        private static MapAzureJson prevMapAzureJson = new MapAzureJson();
        private static MapGitHubJson prevMapGithubJson = new MapGitHubJson();
        private readonly IAPIRepo _repo;
        private string _logFilePath;

        public ChangeLogWrite(IAPIRepo repo, CLGConfig config, LogWriter Logger, string logFilePath)
        {
            _config = config;
            _logger = Logger;
            _fileHandler = new(Logger);
            _reader = new(config.ConfigFilePath, Logger);
            _repo = repo;
            _logFilePath = logFilePath;
            _pathCombiner = new(Logger);
        }

        public async Task<string?> ChangeLogReaderWriter()
        {
            string? EnvVar = null;
            string prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS");

            var organization = _reader.ReadInfo("Organisation");
            var project = _reader.ReadInfo("Project");
            var repositoryName = _reader.ReadInfo("RepositoryName");

            //// Base URL for Azure DevOps REST API
            //string baseUrl = $"https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryName}/commits";

            if (_repo.GetType() == typeof(AzureDevOps))
            {
                EnvVar = Environment.GetEnvironmentVariable("Azure_PAT");

                _client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, APIRepoPath.AzureDevOps, organization, project, "_apis/git/repositories", repositoryName, "commits"),EnvVar,60);
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
            else if (_repo.GetType() == typeof(GitHub))
            {
                EnvVar = Environment.GetEnvironmentVariable("GitHub_PAT");

                _client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, APIRepoPath.Github, organization, project, "_apis/git/repositories", repositoryName, "commits"), EnvVar, 60);
                MapGitHubJson mapJson = await _client.Get<MapGitHubJson>();
                string mapJsonHS = Crc32.CalculateHash<MapGitHubJson>(mapJson);

                if (mapJson != null)
                {
                    if (File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename)))
                    {
                        prevMapGithubJson = _fileHandler.GetJson<MapGitHubJson>(Path.Combine(_config.jsonpath, _config.jsonfilename));
                        if (prevMapGithubJson == null)
                        {
                            prevMapGithubJson = _fileHandler.GetJson<MapGitHubJson>(Path.Combine(_config.backupjsonpath, _config.jsonfilename));
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
            else if (_repo.GetType() == typeof(APITest))
            {
                object? mapJson = null;
                object? prevMapGithubJson = null;
                string mapJsonHS = "";
                object? prevMapJson = "";
                string? testAdd = _config.testClient.BaseAddress.ToString();

                if (_config.runType == "AzureDevOps")
                {
                    _config.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "azure", organization, project, "_apis/git/repositories", repositoryName, "commits"));
                    _testClient = _config.testClient;
                    _client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, testAdd, organization, project, "_apis/git/repositories", repositoryName, "commits"), null, 60, _testClient);
                    mapJson = await _client.Get<MapAzureJson>();
                    mapJsonHS = Crc32.CalculateHash<MapAzureJson>(mapJson as MapAzureJson);
                }
                else if(_config.runType == "GitHub")
                {
                    ///repos/{ owner}/{ repo}/commits
                    _config.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd,"repos", organization, repositoryName, "commits"));
                    _testClient = _config.testClient;
                    _client = new APIClient(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "repos", organization, repositoryName, "commits"), null, 60, _testClient);
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
