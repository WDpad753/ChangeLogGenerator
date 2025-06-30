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
    public class APITest<TEntryPoint> : IAPIRepo<TEntryPoint> where TEntryPoint : class
    {
        private CLGConfig _config;
        private LogWriter _logger;
        private JSONFileHandler _fileHandler;
        private ConfigHandler _reader;
        private AzureDevOps<TEntryPoint> DevOps;
        private GitHub<TEntryPoint> Github;

        public APITest(CLGConfig config, JSONFileHandler JsonReader, ConfigHandler Reader, LogWriter Logger)
        {
            _config = config;
            _logger = Logger;
            _fileHandler = JsonReader;
            _reader = Reader;
            DevOps = new(config,JsonReader,Reader,Logger);
            Github = new(config,JsonReader,Reader,Logger);
        }

        public void MapJsonReader<T>(T mapJson, T prevMapJson, string mapJsonHS, string filepath, APIClient<TEntryPoint>? client = null, string? EnvVar = null)
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

            dynamic jsonData;
            dynamic prevJsonData;

            if (_config.runType == "AzureDevOps")
            {
                jsonData = mapJson as MapAzureJson;
                prevJsonData = prevMapJson as MapAzureJson;
                DevOps.MapJsonReader<MapAzureJson>(jsonData, prevJsonData, mapJsonHS, filepath);
            }
            else if (_config.runType == "GitHub")
            {
                jsonData = mapJson as List<MapGitHubJson>;
                prevJsonData = prevMapJson as List<MapGitHubJson>;
                Github.MapJsonReader<List<MapGitHubJson>>(jsonData, prevJsonData, mapJsonHS, filepath, client, EnvVar);
            }
            else
            {
                throw new NotSupportedException("Unsupported run type");
            }
        }
    }
}
