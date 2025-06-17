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
        //private readonly IConfigReader _reader;
        private ConfigHandler _reader;
        private LogWriter _logger;
        private static MapAzureJson prevMapAzureJson = new MapAzureJson();
        private static MapGitHubJson prevMapGithubJson = new MapGitHubJson();
        private readonly IAPIRepo _repo;
        private string _logFilePath;

        public ChangeLogWrite(CLGConfig config, RepoMode mode, LogWriter Logger, string logFilePath)
        {
            _config = config;
            _logger = Logger;
            _fileHandler = new(Logger);
            _reader = new(_config.ConfigFilePath, Logger);
            _repo = APIFactory.GetAPIRepo(mode, config, _fileHandler, _reader, Logger);
            _logFilePath = logFilePath;
        }

        public async Task<string?> ChangeLogReaderWriter()
        {
            string prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS");

            var organization = _reader.ReadInfo("Organisation");
            var project = _reader.ReadInfo("Project");
            var repositoryName = _reader.ReadInfo("RepositoryName");

            //// Base URL for Azure DevOps REST API
            //string baseUrl = $"https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryName}/commits";

            if (_repo.GetType() == typeof(AzureDevOps))
            {
                string? EnvVar = Environment.GetEnvironmentVariable("Azure_PAT");

                _client = new APIClient(PathCombine.CombinePath(CombinationType.URL, APIRepoPath.AzureDevOps, organization, project, "_apis/git/repositories", repositoryName, "commits"),EnvVar,60);
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
                string? EnvVar = Environment.GetEnvironmentVariable("GitHub_PAT");

                _client = new APIClient(PathCombine.CombinePath(CombinationType.URL, APIRepoPath.Github, organization, project, "_apis/git/repositories", repositoryName, "commits"), EnvVar, 60);
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
                        _repo.MapJsonReader(mapJson, prevMapGithubJson, mapJsonHS, _logFilePath);
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
