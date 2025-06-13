using BaseClass.API;
using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Model;
//using Common.Abstractions;
//using Common.Abstractions.Models;
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
        public string configpathfull
        {
            get
            {
                // Get the current directory (where your executable is located) and set the configpath:
                string currentDirectory = Directory.GetCurrentDirectory();
                string configfpath = @$"{AppDomain.CurrentDomain.BaseDirectory}Config";
                string configfilename = @"App.config";
                string configpath = Path.Combine(configfpath, configfilename);
                return configpath;
            }
        }

        private CLGConfig _config;
        private JSONFileHandler _fileHandler;
        private APIClient _client;
        //private readonly IConfigReader _reader;
        private ConfigReader _reader;
        private LogWriter _logger;
        private static MapJson prevMapJson = new MapJson();
        private readonly IAPIRepo _repo;
        private string _logFilePath;

        public ChangeLogWrite(CLGConfig config, RepoMode mode, LogWriter Logger, string logFilePath)
        {
            _config = config;
            _fileHandler = new();
            _logger = Logger;
            _reader = new(configpathfull, Logger);
            _repo = APIFactory.GetAPIRepo(mode, config, _fileHandler, _reader, _logger);
            _logFilePath = logFilePath;
        }

        public async Task<string> ChangeLogReaderWriter(string filepath)
        {
            string prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS");
            MapJson mapJson = await _client.Get<MapJson>();
            string mapJsonHS = Crc32.CalculateHash(mapJson);

            if (mapJson != null)
            {
                if (File.Exists(Path.Combine(_config.jsonpath, _config.jsonfilename)))
                {
                    prevMapJson = _fileHandler.GetJson<MapJson>(Path.Combine(_config.jsonpath, _config.jsonfilename));
                    if (prevMapJson == null)
                    {
                        prevMapJson = _fileHandler.GetJson<MapJson>(Path.Combine(_config.backupjsonpath, _config.jsonfilename));
                    }
                }

                if (!mapJsonHS.Equals(prevMapJsonHS))
                {
                    _repo.MapJsonReader(mapJson, prevMapJson, mapJsonHS, _logFilePath);
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

            //return sb.ToString();
            return null;
        }
    }
}
