using BaseClass.Config;
using BaseClass.JSON;
using BaseLogger;
using ChangeLogCoreLibrary.APIRepositories.Interface;
using ChangeLogCoreLibrary.Classes;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.APIRepositories.Factory
{
    public class APIFactory<TEntryPoint> where TEntryPoint : class
    {
        public static IAPIRepo<TEntryPoint> GetAPIRepo(RepoMode mode, CLGConfig settings, JSONFileHandler JsonReader, ConfigHandler configReader, LogWriter logger)
        {
            return mode switch
            {
                RepoMode.AzureDevOps => new AzureDevOps<TEntryPoint>(settings, JsonReader, configReader, logger),
                RepoMode.GitHub => new GitHub<TEntryPoint>(settings, JsonReader, configReader, logger),
                _ => throw new ArgumentException("Invalid mode", nameof(mode))
            };
        }
    }
}
