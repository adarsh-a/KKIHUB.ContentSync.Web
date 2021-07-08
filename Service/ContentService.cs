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

		public async Task<List<ContentModel>> FetchContentAsync(string syncId, DateTime startDate, DateTime endDate, string hubId, bool recursive, bool onlyUpdated, string targethub = null)
		{
			try
			{
				var artifacts = await acousticService.FetchArtifactForDateRangeAsync(syncId, startDate, endDate, hubId, recursive, onlyUpdated);

				foreach (var artifact in artifacts)
				{
					var targetItem = await acousticService.FetchContentByIdAsync(artifact.ItemId, targethub);
					if (targetItem == null)
					{
						continue;
					}
					artifact.TargetHubItemName = targetItem.ItemName;
					artifact.TargetHubModifiedDate = targetItem.ModifiedDate;
				}
				return artifacts;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
				return null;
			}

		}
		/// <summary>
		/// fetches content async
		/// </summary>
		/// <param name="fetchContentModel"><see cref="FetchContentModel"></param>
		/// <returns>list of <see cref="ContentModel"></returns>
		public async Task<List<ContentModel>> FetchContentAsync(FetchContentModel fetchContentModel)
		{
			if (fetchContentModel == null)
			{
				throw new ArgumentNullException(nameof(FetchContentModel));
			}
			return await this.FetchContentAsync(fetchContentModel.SyncId, fetchContentModel.StartDate, fetchContentModel.EndDate, fetchContentModel.SourceHub, fetchContentModel.Recursive, fetchContentModel.OnlyUpdated, fetchContentModel.Targethub);
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

		/// <summary>
		/// Fetches modified content by library
		/// </summary>
		/// <param name="days">number of days from now</param>
		/// <param name="hub">hub name</param>
		/// <param name="libraryId">library id</param>
		/// <returns>list of <see cref="ContentModel"></returns>
		public async Task<List<ContentModel>> FetchModifiedContentByLibrary(string syncId, DateTime startdate, DateTime enddate, string hub, string libraryId, string targethub = null)
		{
			try
			{
				var artifacts = await acousticService.FetchArtifactForDateRangeAsync(syncId, startdate, enddate, hub, true, true, libraryId);

				foreach (var artifact in artifacts)
				{
					var targetItem = await acousticService.FetchContentByIdAsync(artifact.ItemId, targethub);
					if (targetItem == null)
					{
						continue;
					}
					artifact.TargetHubItemName = targetItem.ItemName;
					artifact.TargetHubModifiedDate = targetItem.ModifiedDate;
				}
				return artifacts;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.TraceError($"Fetch Artifacts error : {ex.Message}");
				return null;
			}
		}

		public async Task<List<ContentModel>> FetchModifiedContentByLibrary(FetchContentModel fetchContentModel)
		{
			if (fetchContentModel == null)
			{
				throw new ArgumentNullException(nameof(FetchContentModel));
			}
			return await this.FetchModifiedContentByLibrary(fetchContentModel.SyncId, fetchContentModel.StartDate, fetchContentModel.EndDate, fetchContentModel.SourceHub, fetchContentModel.Library, fetchContentModel.Targethub);
		}
	}
}