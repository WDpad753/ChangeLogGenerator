using BaseClass.API.Interface;
using BaseClass.Base;
using BaseClass.Base.Interface;
using BaseClass.Helper;
using BaseClass.Model;
using BaseLogger;
using BaseLogger.Models;
using ChangeLogCoreLibrary.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


namespace ChangeLogCoreLibrary.APIRepositories.Client
{
    public class ClientProvider<T> : IWebFactoryProvider where T : class
    {
        private readonly IBaseSettings _baseSettings;
        private readonly WebApplicationFactory<T>? _factory;
        private readonly ILogger? _logWriter;
        private readonly CLGConfig _config;
        private string? BaseAddress;
        public string? clientBase { get; set; }
        public string? appName { get; set; }


        //public ClientProvider(LogWriter Logger, CLGConfig config, WebApplicationFactory<T>? factory = null)
        //{
        //    _logWriter = Logger;
        //    _config = config;
        //    _factory = factory;
        //}
        public ClientProvider(ILogger? Logger, IBaseSettings settings, CLGConfig config, WebApplicationFactory<T>? factory = null)
        {
            _logWriter = Logger;
            _baseSettings = settings;
            _config = config;
            _factory = factory;
        }

        public HttpClient? CreateClient(Uri? baseAddress)
        {
            try
            {
                HttpClient? client = null;

                if (_factory == null)
                {
                    client = new HttpClient();
                    client.BaseAddress = baseAddress;

                    if (clientBase == null)
                    {
                        throw new Exception("Requires a Client Base");
                    }
                    else
                    {
                        if (clientBase.Equals("AzureDevOps"))
                        {
                            client.BaseAddress = new Uri(PathCombine.CombinePath(CombinationType.URL, APIRepoPath.AzureDevOps, _config.Organisation, _config.Project, "_apis/git/repositories", _config.RepositoryName, "commits").TrimEnd('/'));
                        }
                        else if(clientBase.Equals("GitHub"))
                        {
                            client.BaseAddress = new Uri(PathCombine.CombinePath(CombinationType.URL, APIRepoPath.Github, "repos", _config.Organisation, _config.RepositoryName, "commits").TrimEnd('/'));
                        }
                        else
                        {
                            throw new Exception("Requires a Valid Client Base");
                        }
                    }

                }
                else if (_factory != null)
                {
                    client = _factory.CreateClient();

                    if (clientBase == null)
                    {
                        throw new Exception("Requires a Client Base");
                    }
                    else
                    {
                        if (clientBase.Equals("AzureDevOps"))
                        {
                            client.BaseAddress = new Uri(PathCombine.CombinePath(CombinationType.URL, _factory.Server.BaseAddress.ToString(), _config.Organisation, _config.Project, "_apis/git/repositories", _config.RepositoryName, "commits").TrimEnd('/'));
                        }
                        else if (clientBase.Equals("GitHub"))
                        {
                            client.BaseAddress = new Uri(PathCombine.CombinePath(CombinationType.URL, _factory.Server.BaseAddress.ToString(), "repos", _config.Organisation, _config.RepositoryName, "commits").TrimEnd('/'));
                        }
                        else
                        {
                            throw new Exception("Requires a Valid Client Base");
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Utilising this class requires to know if it is used for testing or for live project");
                }

                _baseSettings.BaseUrlAddress = client.BaseAddress;

                return client;
            }
            catch (Exception ex)
            {
                _logWriter.Error($"Unable to create client. Error Message: {ex.Message}; Trace: {ex.StackTrace}; Exception: {ex.InnerException}; Error Source: {ex.Source}");
                return null;
            }
        }
    }
}
