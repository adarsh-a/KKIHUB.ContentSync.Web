using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KKIHUB.ContentSync.Web.Models
{
    public class SyncModel
    {
        public List<HubInfo> HubInfo { get; set; }

        public SyncModel()
        {
            HubInfo = new List<HubInfo>();
        }
    }
}