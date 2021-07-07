using KKIHUB.ContentSync.Web.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KKIHUB.ContentSync.Web.Service
{
    public interface IAcousticService
    {
        Task<List<ContentModel>> FetchArtifactForDateRangeAsync(DateTime startDate, string hub, bool recursive, bool onlyUpdated,string libraryId = null);

        Task<List<string>> FetchTypeAsync(int days, string hub, bool recursive, bool onlyUpdated);

        Task<List<ContentModel>> FetchContentByLibrary(string hub, string libraryId);

        List<AssetModel> FetchAssetsList();

        /// <summary>
        /// Fetches modified content by library
        /// </summary>
        /// <param name="startdate">start date</param>
        /// <param name="hub">hub name</param>
        /// <param name="libraryId">library id</param>
        /// <returns>list of <see cref="ContentModel"></returns>
        Task<List<ContentModel>> FetchModifiedContentByLibrary(DateTime startdate, string hub, string libraryId);

        Task<ContentModel> FetchContentFromIdAsync(string id, string hub);
    }
}
