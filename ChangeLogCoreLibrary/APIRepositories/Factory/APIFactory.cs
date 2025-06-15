using BaseClass.Config;
using BaseClass.JSON;
using BaseClass.Model;
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
    public class APIFactory
    {
        public static IAPIRepo GetAPIRepo(RepoMode mode, CLGConfig settings, JSONFileHandler JsonReader, ConfigHandler configReader, LogWriter logger)
        {
            return mode switch
            {
                RepoMode.AzureDevOps => new AzureDevOps(settings, JsonReader, configReader, logger),
                RepoMode.GitHub => new GitHub(settings, JsonReader, configReader, logger),
                _ => throw new ArgumentException("Invalid mode", nameof(mode))
            };
        }
    }
}
