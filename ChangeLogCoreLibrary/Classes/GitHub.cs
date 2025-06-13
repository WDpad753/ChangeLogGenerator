using BaseClass.Config;
using BaseClass.JSON;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
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
        private ConfigReader _reader;
        
        public GitHub(CLGConfig config, JSONFileHandler JsonReader, ConfigReader Reader, LogWriter Logger)
        {
            _config = config;
            _logger = Logger;
            _fileHandler = JsonReader;
            _reader = Reader;
        }

        public void MapJsonReader(MapJson mapJson, MapJson prevMapJson, string mapJsonHS, string filepath)
        {

        }

        private string GetLine(int Switch, List<object> jsonvalue)
        {
            return null;
        }

        //public string val = _config.
    }
}
