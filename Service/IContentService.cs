using KKIHUB.ContentSync.Web.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KKIHUB.ContentSync.Web.Service
{
	public interface IContentService
	{
		Task<List<ContentModel>> FetchContentAsync(DateTime startdate, string hubId, bool recursive, bool onlyUpdated, string targethubId = null);

		Task<List<string>> FetchTypeAsync(int days, string hubId, bool recursive, bool onlyUpdated);

		Task<List<ContentModel>> FetchContentByLibrary(string hubId, string libraryId);

		List<AssetModel> FetchAssetsList();

		/// <summary>
		/// Fetches modified content by library
		/// </summary>
		/// <param name="startdate">start date</param>
		/// <param name="hub">hub name</param>
		/// <param name="libraryId">library id</param>
		/// <returns>list of <see cref="ContentModel"></returns>
		Task<List<ContentModel>> FetchModifiedContentByLibrary(DateTime startdate, string hub, string libraryId, string targethub = null);
	}
}
