using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.APIRepositories.Interface
{
    public interface IAPIRepo
    {
        //string GetLine(int Switch, List<object> jsonvalue);
        void MapJsonReader(MapJson mapJson, MapJson prevMapJson, string mapJsonHS, string filepath);
    }
}
