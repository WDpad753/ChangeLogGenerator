using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.Model
{
    public class AzureAuthor
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
    }

    public class ChangeCounts
    {
        public int Add { get; set; }
        public int Delete { get; set; }
        public int Edit { get; set; }
    }

    public class Value
    {
        public AzureAuthor Author { get; set; }
        public ChangeCounts ChangeCounts { get; set; }
        public string Comment { get; set; }
        public string CommitId { get; set; }
        public AzureAuthor Committer { get; set; }
        public string DateChecker { get; set; }
    }

    public class MapAzureJson
    {
        public int Count { get; set; }
        public List<Value> Value { get; set; }
    }
}
