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
        public static IAPIRepo GetAPIRepo(RepoMode mode, CLGConfig settings)
        {
            return mode switch
            {
                RepoMode.AzureDevOps => new AzureDevOps(settings),
                RepoMode.GitHub => new GitHub(settings),
                _ => throw new ArgumentException("Invalid mode", nameof(mode))
            };
        }
    }
}
