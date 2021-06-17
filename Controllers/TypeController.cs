using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using KKIHUB.ContentSync.Web.Service;

namespace KKIHUB.Content.SyncService.Controllers
{
    [Route("api/[controller]")]
    public class TypeController : Controller
    {
        private IContentService ContentService { get; set; }
        public TypeController(IContentService contentService)
        {
            this.ContentService = contentService;
        }


        [HttpGet]
        [Route("Sync")]
        public async Task<ActionResult> SyncContentUpdated(string syncId, string sourceHub)
        {
            var content = await ContentService.FetchTypeAsync(syncId, 0, sourceHub, true, false);

            return Json(content);
        }
    }
}