using BaseClass.API;
using BaseClass.Helper;
using BaseClass.JSON;
using ChangeLogCoreLibrary.APIRepositories.Factory;
using ChangeLogCoreLibrary.Model;
using Common.Abstractions;
using Common.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogConsole.Writer
{
    public class ChangeLogWrite
    {
        private string configpathfull
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
        private readonly IConfigReader _reader;

        public ChangeLogWrite(CLGConfig config, RepoMode mode,DebugState state)
        {
            _config = config;
            _fileHandler = new(state);
            var APIMode = APIFactory.GetAPIRepo(mode,config);
            //_client = new();
        }

        public async Task<string> ChangeLogReaderWriter(string filepath)
        {
            string line;
            int Switch;
            string prevdatetime = "";
            int firstentry = 0;
            string prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS","");
            //MapJson prevMapJson;
            int addition;
            int alteration;
            int deletion;
            int printcount = 0;
            MapJson mapJson = await _client.Get<MapJson>();
            //Crc32 crc = new Crc32();
            //string mapJsonHS = crc.CalculateHash(mapJson);
            string mapJsonHS = Crc32.CalculateHash(mapJson);
            List<object> values;
            //List<object> JsonMapValues = new List<object>();
            Dictionary<long, List<object>> PrevJsonMap = new Dictionary<long, List<object>>();
            Dictionary<long, List<object>> JsonMap = new Dictionary<long, List<object>>();


            return null;
        }

    }
}
