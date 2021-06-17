using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KKIHUB.ContentSync.Web.Model
{
    public class PushParams
    {
        public List<string> FilePaths { get; set; }
        public string TargetHub { get; set; }
    }
}
