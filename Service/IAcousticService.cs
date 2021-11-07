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
		Task<List<ContentModel>> FetchArtifactForDateRangeAsync(string syncId, DateTime startDate,DateTime endDate, string hub, bool recursive, bool onlyUpdated, string libraryId = null);

		Task<List<string>> FetchTypeAsync(string syncId, int days, string hub, bool recursive, bool onlyUpdated);

		Task<List<ContentModel>> FetchContentByLibrary(string syncId, string hub, string libraryId);

		List<AssetModel> FetchAssetsList();

		Task<ContentModel> FetchContentByIdAsync(string id, string hub);
	}
}
