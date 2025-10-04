﻿using BaseClass.Base.Interface;
using BaseClass.Config;
using BaseClass.ConsoleAppBase;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Client;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Classes;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogConsole.Base
{
    public class ChangeLogBase<T> where T : class
    {
        private readonly IBaseProvider? _provider;
        private CLGConfig _config;
        private JSONFileHandler _fileHandler;
        private APIClient<T> _client;
        private ConfigHandler _reader;
        private EnvHandler _envreader;
        private ILogger? _logger;
        private static MapAzureJson prevMapAzureJson = new MapAzureJson();
        private static List<MapGitHubJson> prevMapGithubJson = new List<MapGitHubJson>();
        private readonly IAPIRepo<T> _repo;
        private string? _logFilePath;
        private readonly ClientProvider<T>? _factoryProvider;

        public ChangeLogBase(IBaseProvider provider)
        {
            _provider = provider;
        }

        public async Task<string?> ChangeLogReaderWriter()
        {
            string? EnvVar = null;
            string? Envvar = null;
            string? prevMapJsonHS = null;
            string? organization = null;
            string? project = null;
            string? repositoryName = null;

            prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS", "changelogSettings");
            organization = _reader.ReadInfo("Organisation", "changelogSettings");
            project = _reader.ReadInfo("Project", "changelogSettings");
            repositoryName = _reader.ReadInfo("RepositoryName", "changelogSettings");
            Envvar = _reader.ReadInfo("PAT", "changelogSettings");

            _config.Organisation = organization;
            _config.Project = project;
            _config.RepositoryName = repositoryName;

            try
            {
                if (_repo.GetType() == typeof(AzureDevOps<T>))
                {
                    EnvVar = Envvar == "" ? null : Environment.GetEnvironmentVariable(Envvar);

                    //_client.APIURL = PathCombine.CombinePath(CombinationType.URL, APIRepoPath.AzureDevOps, organization, project, "_apis/git/repositories", repositoryName, "commits");
                    _client.PerAccTok = EnvVar;
                    _client.timeOut = 60;
                    _factoryProvider.clientBase = "AzureDevOps";

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
                    EnvVar = Envvar == "" ? null : _envreader.EnvRead(Envvar, EnvAccessMode.User);
                    //var val = Environment.GetEnvironmentVariable(Envvar);
                    //_client.APIURL = PathCombine.CombinePath(CombinationType.URL, APIRepoPath.Github, "repos", organization, repositoryName, "commits").TrimEnd('/');
                    _client.PerAccTok = EnvVar;
                    _client.timeOut = 60;
                    _factoryProvider.clientBase = "GitHub";

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

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($@"Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
                return null;
            }
        }
    }
}
