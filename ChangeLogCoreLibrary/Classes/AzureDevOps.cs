using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.APIRepositories.Client;
using ChangeLogCoreLibrary.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseClass.Base.Interface;

namespace ChangeLogCoreLibrary.Classes
{
    public class AzureDevOps<TEntryPoint> : IAPIRepo<TEntryPoint> where TEntryPoint : class
    {
        private readonly IBase? baseConfig;
        private CLGConfig _config;
        private ILogger _logger;
        private JSONFileHandler _fileHandler;
        private ConfigHandler _reader;
        private static MapAzureJson prevMapJson = new MapAzureJson();

        //public AzureDevOps(CLGConfig config, JSONFileHandler JsonReader, ConfigHandler configReader,  LogWriter Logger)
        //{
        //    _config = config;
        //    _logger = Logger;
        //    _fileHandler = JsonReader;
        //    _reader = configReader;
        //}
        public AzureDevOps(CLGConfig config, IBase? BaseConfig)
        {
            _config = config;
            _logger = BaseConfig.Logger;
            _fileHandler = BaseConfig.JSONFileHandler;
            _reader = BaseConfig.ConfigHandler;
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
            string prevMapJsonHS = _reader.ReadInfo("PrevMapJSONHS", "changelogSettings");
            int printcount = 0;
            var jsonData = mapJson as MapAzureJson;
            var prevJsonData = prevMapJson as MapAzureJson;

            if (mapJson == null)
            {
                return;
            }
            else
            {
                foreach (var value in jsonData.Value)
                {
                    List<object> JsonMapValues = new List<object>();
                    value.DateChecker = DateTime.Parse(value.Author.Date.ToString()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    JsonMapValues.Add(DateTime.Parse(value.Author.Date.ToString()));
                    JsonMapValues.Add(value.CommitId);
                    JsonMapValues.Add(value.Author.Name);
                    JsonMapValues.Add(value.Committer.Name);
                    JsonMapValues.Add(value.Comment);
                    JsonMapValues.Add(value.ChangeCounts);
                    JsonMapValues.Add(value.DateChecker);
                    JsonMap.Add(DateTimeOffset.Parse(value.Author.Date.ToString()).ToUnixTimeSeconds(), JsonMapValues);
                }

                Dictionary<long, List<object>> ascOrderedJsonMap = JsonMap.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);

                List<List<object>> listOfLists = ascOrderedJsonMap.Values.ToList();

                FileInfo fileInfo = new FileInfo(filepath);
                if (File.Exists(filepath) && fileInfo.Length <= 0)
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        foreach (object valueItem in listOfLists)
                        {
                            if (valueItem is IList<object> list && list.Count >= 5)
                            {
                                var Datetime = list[0].ToString();
                                var CommitID = list[1].ToString();
                                var AuthorName = list[2].ToString();
                                var CommitterName = list[3].ToString();
                                var CommitMessage = list[4].ToString();
                                var ChangeCounts = list[5];

                                DateTime dt = DateTime.Parse(Datetime);
                                string date = dt.ToString("dddd, yyyy-MM-dd", CultureInfo.InvariantCulture);
                                string datecheck = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                                string time = dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

                                if (ChangeCounts is ChangeCounts) // Check if the object is of type dataclass1
                                {
                                    ChangeCounts cc = (ChangeCounts)ChangeCounts;
                                    addition = cc.Add;
                                    alteration = cc.Edit;
                                    deletion = cc.Delete;
                                }
                                else
                                {
                                    throw new Exception("Cannot find the ChangeCounts in the List.");
                                }

                                if (datecheck == prevdatetime)
                                {
                                    //firstentry = 0;
                                }
                                else
                                {
                                    if (firstentry > 0)
                                    {
                                        sw.WriteLine();
                                        sw.WriteLine();
                                    }

                                    firstentry++;
                                    sw.WriteLine($"Date: {date}");
                                }

                                values = new List<object> { time, CommitID, AuthorName, CommitterName, CommitMessage };

                                if (addition > 0 && alteration == 0 && deletion == 0)
                                {
                                    Switch = 0;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition == 0 && alteration > 0 && deletion == 0)
                                {
                                    Switch = 1;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition == 0 && alteration == 0 && deletion > 0)
                                {
                                    Switch = 2;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition > 0 && alteration > 0 && deletion == 0)
                                {
                                    Switch = 3;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition > 0 && alteration == 0 && deletion > 0)
                                {
                                    Switch = 4;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition == 0 && alteration > 0 && deletion > 0)
                                {
                                    Switch = 5;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition > 0 && alteration > 0 && deletion > 0)
                                {
                                    Switch = 6;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else
                                {
                                    Switch = default;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }

                                prevdatetime = datecheck;
                            }
                        }
                        sw.Close();
                    }

                    _fileHandler.SaveJson(mapJson, Path.Combine(_config.jsonpath, _config.jsonfilename).ToString());
                    prevMapJsonHS = Crc32.CalculateHash(mapJson);
                    _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");
                }
                else if (File.Exists(filepath) && fileInfo.Length > 0 && !mapJsonHS.Equals(prevMapJsonHS))
                {
                    JsonMap = new Dictionary<long, List<object>>();
                    foreach (var value in prevJsonData.Value)
                    {
                        List<object> JsonMapValues = new List<object>();
                        value.DateChecker = DateTime.Parse(value.Author.Date.ToString()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        JsonMapValues.Add(DateTime.Parse(value.Author.Date.ToString()));
                        JsonMapValues.Add(value.CommitId);
                        JsonMapValues.Add(value.Author.Name);
                        JsonMapValues.Add(value.Committer.Name);
                        JsonMapValues.Add(value.Comment);
                        JsonMapValues.Add(value.ChangeCounts);
                        JsonMapValues.Add(value.DateChecker);
                        JsonMap.Add(DateTimeOffset.Parse(value.Author.Date.ToString()).ToUnixTimeSeconds(), JsonMapValues);
                    }

                    Dictionary<long, List<object>> prevAscOrderedJsonMap = JsonMap.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);

                    List<List<object>> listOfLists2 = ascOrderedJsonMap.Values.ToList();

                    foreach (var value in ascOrderedJsonMap.Keys)
                    {
                        printcount++;
                        if (prevAscOrderedJsonMap.ContainsKey(value))
                        {
                            Console.WriteLine("File Exists and it does not need appending to the current text");
                            List<object> prevval = new List<object>();
                            prevval = prevAscOrderedJsonMap[value];
                            prevdatetime = prevval[6].ToString();
                        }
                        else
                        {
                            Console.WriteLine("File Exists and it needs appending to the current text");
                            using (StreamWriter sw = File.AppendText(filepath))
                            {
                                if (!prevAscOrderedJsonMap.ContainsKey(value))
                                {
                                    if (listOfLists2[printcount - 1] is IList<object> list && list.Count >= 5)
                                    {
                                        var Datetime = list[0].ToString();
                                        var CommitID = list[1].ToString();
                                        var AuthorName = list[2].ToString();
                                        var CommitterName = list[3].ToString();
                                        var CommitMessage = list[4].ToString();
                                        var ChangeCounts = list[5];

                                        DateTime dt = DateTime.Parse(Datetime);
                                        string date = dt.ToString("dddd, yyyy-MM-dd", CultureInfo.InvariantCulture);
                                        string datecheck = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                                        string time = dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

                                        if (ChangeCounts is ChangeCounts) // Check if the object is of type dataclass1
                                        {
                                            ChangeCounts cc = (ChangeCounts)ChangeCounts;
                                            addition = cc.Add;
                                            alteration = cc.Edit;
                                            deletion = cc.Delete;
                                        }
                                        else
                                        {
                                            throw new Exception("Cannot find the ChangeCounts in the List.");
                                        }

                                        if (datecheck.Equals(prevdatetime))
                                        {
                                            //firstentry = 0;
                                        }
                                        else
                                        {
                                            if (firstentry == 0)
                                            {
                                                sw.WriteLine();
                                                sw.WriteLine();
                                            }

                                            firstentry++;
                                            sw.WriteLine($"Date: {date}");
                                        }

                                        values = new List<object> { time, CommitID, AuthorName, CommitterName, CommitMessage };

                                        if (addition > 0 && alteration == 0 && deletion == 0)
                                        {
                                            Switch = 0;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition == 0 && alteration > 0 && deletion == 0)
                                        {
                                            Switch = 1;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition == 0 && alteration == 0 && deletion > 0)
                                        {
                                            Switch = 2;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition > 0 && alteration > 0 && deletion == 0)
                                        {
                                            Switch = 3;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition > 0 && alteration == 0 && deletion > 0)
                                        {
                                            Switch = 4;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition == 0 && alteration > 0 && deletion > 0)
                                        {
                                            Switch = 5;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition > 0 && alteration > 0 && deletion > 0)
                                        {
                                            Switch = 6;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else
                                        {
                                            Switch = default;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }

                                        prevdatetime = datecheck;
                                    }
                                }

                                _fileHandler.SaveJson(mapJson, Path.Combine(_config.jsonpath, _config.jsonfilename).ToString());
                                prevMapJsonHS = Crc32.CalculateHash(mapJson);
                                _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");
                                sw.Close();
                            }
                            Console.WriteLine("Done");
                        }
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(PathCombine.CombinePath(CombinationType.Folder, filepath, _config.logfilename)))
                    {
                        foreach (object valueItem in listOfLists)
                        {
                            if (valueItem is IList<object> list && list.Count >= 5)
                            {
                                var Datetime = list[0].ToString();
                                var CommitID = list[1].ToString();
                                var AuthorName = list[2].ToString();
                                var CommitterName = list[3].ToString();
                                var CommitMessage = list[4].ToString();
                                var ChangeCounts = list[5];

                                DateTime dt = DateTime.Parse(Datetime);
                                string date = dt.ToString("dddd, yyyy-MM-dd", CultureInfo.InvariantCulture);
                                string datecheck = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                                string time = dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

                                if (ChangeCounts is ChangeCounts) // Check if the object is of type dataclass1
                                {
                                    ChangeCounts cc = (ChangeCounts)ChangeCounts;
                                    addition = cc.Add;
                                    alteration = cc.Edit;
                                    deletion = cc.Delete;
                                }
                                else
                                {
                                    throw new Exception("Cannot find the ChangeCounts in the List.");
                                }

                                if (datecheck == prevdatetime)
                                {
                                    //firstentry = 0;
                                }
                                else
                                {
                                    if (firstentry > 0)
                                    {
                                        writer.WriteLine();
                                        writer.WriteLine();
                                    }
                                    firstentry++;
                                    writer.WriteLine($"Date: {date}");
                                }

                                List<object> value = new List<object> { time, CommitID, AuthorName, CommitterName, CommitMessage };

                                if (addition > 0 && alteration == 0 && deletion == 0)
                                {
                                    Switch = 0;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }
                                else if (addition == 0 && alteration > 0 && deletion == 0)
                                {
                                    Switch = 1;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }
                                else if (addition == 0 && alteration == 0 && deletion > 0)
                                {
                                    Switch = 2;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }
                                else if (addition > 0 && alteration > 0 && deletion == 0)
                                {
                                    Switch = 3;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }
                                else if (addition > 0 && alteration == 0 && deletion > 0)
                                {
                                    Switch = 4;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }
                                else if (addition == 0 && alteration > 0 && deletion > 0)
                                {
                                    Switch = 5;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }
                                else if (addition > 0 && alteration > 0 && deletion > 0)
                                {
                                    Switch = 6;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }
                                else
                                {
                                    Switch = default;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }

                                prevdatetime = datecheck;
                            }
                        }

                        _fileHandler.SaveJson(mapJson, Path.Combine(_config.jsonpath, _config.jsonfilename).ToString());
                        prevMapJsonHS = Crc32.CalculateHash(mapJson);
                        _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");
                        writer.Close();
                    }
                }
            }
        }

        private string GetLine(int Switch, List<object> jsonvalue)
        {
            string line = "";
            switch (Switch)
            {
                case 0:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Addition";
                    return line;
                case 1:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Alterations";
                    return line;
                case 2:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Deletion";
                    return line;
                case 3:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Addition, Alterations";
                    return line;
                case 4:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Addition, Deletion";
                    return line;
                case 5:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Alterations, Deletion";
                    return line;
                case 6:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Addition, Alterations, Deletion";
                    return line;
                default:
                    line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};";
                    return line;
            }
        }
    }
}
