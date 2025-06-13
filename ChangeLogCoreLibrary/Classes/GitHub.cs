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
        
        public GitHub(CLGConfig config) 
        {
            _config = config;
        }

        //public string val = _config.
    }
}
