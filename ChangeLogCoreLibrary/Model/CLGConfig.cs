using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.Model
{
    public class CLGConfig
    {
        public string? jsonpath;
        public string? backupjsonpath;
        public string? jsonfilename;
        public string? prevMapJsonHS;
        public string? testURL;

        public string? ConfigFilePath { get; set; }

        public string? logfilepath { get; set; }
        public string? logfilename { get; set; }

        public string? JsonFilePath { get; set; }

        public string? runType { get; set; }

        public HttpClient? testClient { get; set; }
    }
}
