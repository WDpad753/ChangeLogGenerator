using BaseClass.API;
using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.Classes
{
    public class GitHub : IAPIRepo
    {
        private CLGConfig _config;
        private LogWriter _logger;
        private JSONFileHandler _fileHandler;
        private ConfigHandler _reader;
        private PathCombine _pathCombiner;
        //private APIClient _client;

        public GitHub(CLGConfig config, JSONFileHandler JsonReader, ConfigHandler Reader, LogWriter Logger)
        {
            _config = config;
            _logger = Logger;
            _fileHandler = JsonReader;
            _reader = Reader;
            _pathCombiner = new(Logger);
        }

        public void MapJsonReader<T>(T mapJson, T prevMapJson, string mapJsonHS, string filepath, APIClient? client = null, string? EnvVar = null)
        {
            int Switch;
            Dictionary<long, List<object>> JsonMap = new Dictionary<long, List<object>>();
            int addition;
            int alteration;
            int deletion;
            string prevdatetime = "";
            int firstentry = 0;
            List<object> values;
            string line;
            string prevMapJsonHS = "";
            int printcount = 0;
            var jsonData = mapJson as List<MapGitHubJson>;
            var prevJsonData = prevMapJson as List<MapGitHubJson>;

            if (mapJson == null)
            {
                return;
            }
            else
            {
                foreach (var value in jsonData)
                {
                    List<object> JsonMapValues = new List<object>();
                    value.DateChecker = DateTime.Parse(value.commit.author.date.ToString()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    JsonMapValues.Add(DateTime.Parse(value.commit.author.date.ToString()));
                    JsonMapValues.Add(value.committer.id);
                    JsonMapValues.Add(value.sha);
                    JsonMapValues.Add(value.committer.name);
                    JsonMapValues.Add(value.commit.message);

                    //client.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "azure", organization, project, "_apis/git/repositories", repositoryName, "commits"));

                    if(client.testClient != null)
                    {
                        client.testClient.BaseAddress = new Uri(value.url);

                        HttpClient testClient = client.testClient;
                        client.testClient = testClient;
                    }
                    else
                    {
                        client.APIURL = value.url;
                    }

                    var jsonFile = client.Get<MapGitHubCommitJson>(value.url);

                    JsonMapValues.Add(jsonFile.Status);
                    JsonMapValues.Add(value.DateChecker);
                    JsonMap.Add(DateTimeOffset.Parse(value.author.date.ToString()).ToUnixTimeSeconds(), JsonMapValues);
                }
            }
        }

        private string GetLine(int Switch, List<object> jsonvalue)
        {
            return null;
        }

        //public string val = _config.
    }
}
