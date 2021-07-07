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

		public async Task<List<ContentModel>> FetchContentAsync(DateTime startdate, string hubId, bool recursive, bool onlyUpdated, string targethubId = null)
		{
			try
			{
				var artifacts = await acousticService.FetchArtifactForDateRangeAsync(startdate, hubId, recursive, onlyUpdated);
				foreach (var artifact in artifacts)
				{
					var aa = await acousticService.FetchContentFromIdAsync(artifact.ItemId, targethubId);
				}
				return artifacts;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
				return null;
			}

		}

		public async Task<List<string>> FetchTypeAsync(int days, string hubId, bool recursive, bool onlyUpdated)
		{
			try
			{
				var artifacts = await acousticService.FetchTypeAsync(days, hubId, recursive, onlyUpdated);
				return artifacts;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
				return null;
			}

		}


		public async Task<List<ContentModel>> FetchContentByLibrary(string hubId, string libraryId)
		{
			try
			{
				var artifacts = await acousticService.FetchContentByLibrary(hubId, libraryId);
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

		/// <summary>
		/// Fetches modified content by library
		/// </summary>
		/// <param name="days">number of days from now</param>
		/// <param name="hub">hub name</param>
		/// <param name="libraryId">library id</param>
		/// <returns>list of <see cref="ContentModel"></returns>
		public async Task<List<ContentModel>> FetchModifiedContentByLibrary(DateTime startdate, string hub, string libraryId, string targethub = null)
		{
			try
			{
				var artifacts = await acousticService.FetchArtifactForDateRangeAsync(startdate, hub, true, true, libraryId);
				foreach (var artifact in artifacts)
				{
					var aa = await acousticService.FetchContentFromIdAsync(artifact.ItemId, targethub);
				}
				return artifacts;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
				return null;
			}
		}
	}
}
