using KKIHUB.ContentSync.Web.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KKIHUB.ContentSync.Web.Service
{
    public class ContentService : IContentService
    {
        private IAcousticService acousticService;
        public ContentService(IAcousticService acousticService)
        {
            this.acousticService = acousticService;
        }

        public async Task<List<ContentModel>> FetchContentAsync(string syncId, int days, string hubId, bool recursive, bool onlyUpdated)
        {
            try
            {
                var artifacts = await acousticService.FetchArtifactForDateRangeAsync(syncId, days, hubId, recursive, onlyUpdated);
                return artifacts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
                return null;
            }

        }

        public async Task<List<string>> FetchTypeAsync(string syncId, int days, string hubId, bool recursive, bool onlyUpdated)
        {
            try
            {
                var artifacts = await acousticService.FetchTypeAsync(syncId, days, hubId, recursive, onlyUpdated);
                return artifacts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
                return null;
            }

        }


        public async Task<List<ContentModel>> FetchContentByLibrary(string syncId, string hubId, string libraryId)
        {
            try
            {
                var artifacts = await acousticService.FetchContentByLibrary(syncId, hubId, libraryId);
                return artifacts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
                return null;
            }

        }

        public List<AssetModel> FetchAssetsList()
        {
            try
            {
                var assetsList = acousticService.FetchAssetsList();
                return assetsList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
                return null;
            }

        }

    }
}