using BaseClass.API;
using BaseClass.Config;
using BaseClass.Helper;
using BaseClass.JSON;
using BaseClass.Model;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Helper;
using ChangeLogCoreLibrary.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crc32 = BaseClass.Helper.Crc32;

namespace ChangeLogCoreLibrary.Classes
{
    public class GitHub<TEntryPoint> : IAPIRepo<TEntryPoint> where TEntryPoint : class
    {
        private CLGConfig _config;
        private LogWriter _logger;
        private JSONFileHandler _fileHandler;
        private ConfigHandler _reader;
        private PathCombine _pathCombiner;
        //private APIClient _client;
        private Guid commiterID;

        public GitHub(CLGConfig config, JSONFileHandler JsonReader, ConfigHandler Reader, LogWriter Logger)
        {
            _config = config;
            _logger = Logger;
            _fileHandler = JsonReader;
            _reader = Reader;
            _pathCombiner = new(Logger);
            commiterID = Guid.NewGuid();
        }

        public async void MapJsonReader<T>(T mapJson, T prevMapJson, string mapJsonHS, string filepath, APIClient<TEntryPoint>? client = null, string? EnvVar = null)
        {
            int Switch;
            Dictionary<long, List<object>> JsonMap = new Dictionary<long, List<object>>();
            int? addition = null;
            int alteration;
            int? deletion = null;
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
                    JsonMapValues.Add(value.committer == null ? commiterID.ToString().Replace("-","") : value.committer.id);
                    JsonMapValues.Add(value.commit.author.name);
                    JsonMapValues.Add(value.commit.author.name);
                    JsonMapValues.Add(value.commit.message);
                    MapGitHubCommitJson? jsonFile = null;

                    if (_config.testURL != null)
                    {
                        if (_config.testClient.BaseAddress.ToString().Contains(_config.testURL))
                        {
                            jsonFile = await client.Get<MapGitHubCommitJson>(value.sha);
                        }
                        else
                        {
                            jsonFile = await client.Get<MapGitHubCommitJson>(value.url);
                        }
                    }
                    else if(_config.testURL == null && _config.testClient != null)
                    {
                        jsonFile = await client.Get<MapGitHubCommitJson>(value.url);
                    }

                    if (_config.testClient == null)
                    {
                        jsonFile = await client.Get<MapGitHubCommitJson>(value.sha);
                    }

                    //jsonFile = await client.Get<MapGitHubCommitJson>(value.url);

                    JsonMapValues.Add(jsonFile.files);
                    //JsonMapValues.Add(jsonFile.files);
                    JsonMapValues.Add(value.DateChecker);
                    JsonMap.Add(DateTimeOffset.Parse(value.commit.author.date.ToString()).ToUnixTimeSeconds(), JsonMapValues);
                }

                Dictionary<long, List<object>> ascOrderedJsonMap = JsonMap.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                //Dictionary<long, List<object>> ascOrderedJsonMap = JsonMap.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value.Distinct().ToList());

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

                                if (addition > 0 && deletion == 0)
                                {
                                    Switch = 0;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition == 0 && deletion > 0)
                                {
                                    Switch = 1;
                                    line = GetLine(Switch, values);
                                    sw.WriteLine(line);
                                }
                                else if (addition > 0 && deletion > 0)
                                {
                                    Switch = 2;
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
                    //_fileHandler.SaveJson(mapJson, Path.Combine(_config.backupjsonpath, _config.jsonfilename).ToString());
                    prevMapJsonHS = Crc32.CalculateHash(mapJson);
                    
                    if(_config.testClient != null)
                    {
                        _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS");
                    }
                    else
                    {
                        _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");
                    }
                }
                else if (File.Exists(filepath) && fileInfo.Length > 0 && !mapJsonHS.Equals(prevMapJsonHS))
                {
                    JsonMap = new Dictionary<long, List<object>>();
                    foreach (var value in prevJsonData)
                    {
                        List<object> JsonMapValues = new List<object>();
                        value.DateChecker = DateTime.Parse(value.commit.author.date.ToString()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        JsonMapValues.Add(DateTime.Parse(value.commit.author.date.ToString()));
                        JsonMapValues.Add(value.committer.id);
                        JsonMapValues.Add(value.sha);
                        JsonMapValues.Add(value.committer.name);
                        JsonMapValues.Add(value.commit.message);

                        //client.testClient.BaseAddress = new Uri(_pathCombiner.CombinePath(CombinationType.URL, testAdd, "azure", organization, project, "_apis/git/repositories", repositoryName, "commits"));

                        //if (client.testClient == true)
                        //{
                        //    client.APIURL = value.sha;
                        //}
                        //if (client.testClient != true)
                        //{
                        //    //client.testClient.BaseAddress = new Uri(value.url);

                        //    //HttpClient testClient = client.testClient;
                        //    //client.testClient = testClient;
                        //}
                        //else
                        //{
                        //    client.APIURL = value.url;
                        //}

                        var jsonFile = await client.Get<MapGitHubCommitJson>(value.url);

                        JsonMapValues.Add(jsonFile.stats);
                        JsonMapValues.Add(value.DateChecker);
                        JsonMap.Add(DateTimeOffset.Parse(value.commit.author.date.ToString()).ToUnixTimeSeconds(), JsonMapValues);
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

                                        if (addition > 0 && deletion == 0)
                                        {
                                            Switch = 0;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition == 0 && deletion > 0)
                                        {
                                            Switch = 1;
                                            line = GetLine(Switch, values);
                                            sw.WriteLine(line);
                                        }
                                        else if (addition > 0 && deletion > 0)
                                        {
                                            Switch = 2;
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
                                //_fileHandler.SaveJson(mapJson, Path.Combine(_config.backupjsonpath, _config.jsonfilename).ToString());
                                prevMapJsonHS = Crc32.CalculateHash(mapJson);
                                //_reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");

                                if (_config.testClient != null)
                                {
                                    _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS");
                                }
                                else
                                {
                                    _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");
                                }

                                sw.Close();
                            }
                            Console.WriteLine("Done");
                        }
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(_pathCombiner.CombinePath(CombinationType.Folder, filepath, _config.logfilename)))
                    {
                        int cnt = 0;

                        foreach (object valueItem in listOfLists)
                        {
                            if (valueItem is IList<object> list && list.Count >= 5)
                            {
                                var Datetime = list[0].ToString();
                                var CommitID = list[1].ToString();
                                var AuthorName = list[2].ToString();
                                var CommitterName = list[3].ToString();
                                var CommitMessage = list[4].ToString();
                                //var ChangeCounts = list[5];
                                var ChangeCounts = list.OfType<GitFile[]>().FirstOrDefault();

                                DateTime dt = DateTime.Parse(Datetime);
                                string date = dt.ToString("dddd, yyyy-MM-dd", CultureInfo.InvariantCulture);
                                string datecheck = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                                string time = dt.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

                                if (ChangeCounts.Length > 0)
                                {
                                    string? prevChangeCountHS = null;

                                    foreach (var ChangeCount in ChangeCounts)
                                    {
                                        GitFile cc = ChangeCount;

                                        string changeCountHS = Crc32.CalculateHash<GitFile>(cc);

                                        if(changeCountHS == prevChangeCountHS)
                                        {
                                            continue;
                                        }

                                        addition = cc.additions;
                                        alteration = cc.changes;
                                        deletion = cc.deletions;

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
                                        prevChangeCountHS = changeCountHS;
                                    }
                                }
                                else
                                {
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

                                    Switch = default;
                                    line = GetLine(Switch, value);
                                    writer.WriteLine(line);
                                }

                                prevdatetime = datecheck;
                                cnt++;
                            }
                        }

                        _fileHandler.SaveJson(mapJson, Path.Combine(_config.jsonpath, _config.jsonfilename).ToString());
                        //_fileHandler.SaveJson(mapJson, Path.Combine(_config.backupjsonpath, _config.jsonfilename).ToString());
                        prevMapJsonHS = Crc32.CalculateHash(mapJson);
                        //_reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");
                        //writer.Close();

                        if (_config.testClient != null)
                        {
                            _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS");
                        }
                        else
                        {
                            _reader.SaveInfo(prevMapJsonHS, "PrevMapJSONHS", "changelogSettings");
                        }

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

        //private string GetLine(int Switch, List<object> jsonvalue)
        //{
        //    string line = "";
        //    switch (Switch)
        //    {
        //        case 0:
        //            line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Addition";
        //            return line;
        //        case 1:
        //            line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Deletion";
        //            return line;
        //        case 2:
        //            line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};        Changes Done: Addition, Deletion";
        //            return line;
        //        default:
        //            line = $"Time: {jsonvalue[0]}; CommitID {jsonvalue[1]}; Author: {jsonvalue[2]}({jsonvalue[3]}); Commit Message: {jsonvalue[4]};";
        //            return line;
        //    }
        //    //return null;
        //}

        //public string val = _config.
    }
}
