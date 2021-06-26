using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KKIHUB.ContentSync.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var syncModel = new Models.SyncModel();
            var hubMap = Constants.Constants.HubNameToId;
            foreach (var hub in hubMap)
            {
                syncModel.HubInfo.Add(new Models.HubInfo { HubName = hub.Key, HubId = hub.Value });
            }
            return View(syncModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}