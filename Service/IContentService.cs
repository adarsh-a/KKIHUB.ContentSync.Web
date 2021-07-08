using KKIHUB.ContentSync.Web.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KKIHUB.ContentSync.Web.Service
{
	public interface IContentService
	{
		Task<List<ContentModel>> FetchContentAsync(string syncId, DateTime startDate,DateTime endDate, string hubId, bool recursive, bool onlyUpdated, string library = null);

		/// <summary>
		/// fetches content async
		/// </summary>
		/// <param name="fetchContentModel"><see cref="FetchContentModel"></param>
		/// <returns>list of <see cref="ContentModel"></returns>
		Task<List<ContentModel>> FetchContentAsync(FetchContentModel fetchContentModel);

		Task<List<string>> FetchTypeAsync(string syncId, int days, string hubId, bool recursive, bool onlyUpdated);

		Task<List<ContentModel>> FetchContentByLibrary(string syncId, string hubId, string libraryId);

		List<AssetModel> FetchAssetsList();

		/// <summary>
		/// Fetches modified content by library
		/// </summary>
		/// <param name="syncId">sync id</param>
		/// <param name="startdate">start date</param>
		/// <param name="hub">hub name</param>
		/// <param name="libraryId">library id</param>
		/// <returns>list of <see cref="ContentModel"></returns>
		Task<List<ContentModel>> FetchModifiedContentByLibrary(string syncId, DateTime startdate, DateTime enddate, string hub, string libraryId, string targethub = null);

		Task<List<ContentModel>> FetchModifiedContentByLibrary(FetchContentModel fetchContentModel);
	}
}
