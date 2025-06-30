using BaseClass.API;
using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.APIRepositories.Interface
{
    public interface IAPIRepo<TEntryPoint> where TEntryPoint : class
    {
        //string GetLine(int Switch, List<object> jsonvalue);
        void MapJsonReader<T>(T mapJson, T prevMapJson, string mapJsonHS, string filepath, APIClient<TEntryPoint>? client = null, string? EnvVar = null);
    }
}
