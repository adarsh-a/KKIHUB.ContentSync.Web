using KKIHUB.ContentSync.Web.Helper;
using KKIHUB.ContentSync.Web.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KKIHUB.ContentSync.Web.Service
{
	public class AcousticService : IAcousticService
	{
		private List<string> ItemsFetched = new List<string>();
		private List<ContentModel> ContentModelList = new List<ContentModel>();
		private List<string> AssociatedAssetsId = new List<string>();
		private static List<AssetModel> AssetModelList = new List<AssetModel>();

		public async Task<List<ContentModel>> FetchArtifactForDateRangeAsync(DateTime startDate, string hub, bool recursive, bool onlyUpdated, string libraryId = null)
		{
			ItemsFetched.Add($"Sync Started at {DateTime.Now}");
			if (Constants.Constants.HubNameToId.ContainsKey(hub)
				&& Constants.Constants.HubToApi.ContainsKey(hub))
			{
				var hubId = Constants.Constants.HubNameToId[hub];
				var hubApi = Constants.Constants.HubToApi[hub];
				try
				{
					var baseUrl = Constants.Constants.Endpoints.Base.Replace("{hubId}", hubId);
					var dateRangeUrl = $"{baseUrl}{ Constants.Constants.Endpoints.FetchContentDateRange}";
					var contentIdUrl = $"{baseUrl}{ Constants.Constants.Endpoints.FetchContentById}";

					var startdate = startDate.ToUniversalTime().ToString("o");
					var endDate = DateTime.UtcNow.ToString("o");

					var parameters = $"start={startdate}&end={endDate}&format=sequence";
					dateRangeUrl = $"{dateRangeUrl}?{parameters}";

					var request = WebRequest.Create(new Uri(dateRangeUrl));
					request.Method = "GET";

					//request.Credentials = new NetworkCredential("AcousticAPIKey", hupApi);

					string credidentials = "AcousticAPIKey" + ":" + hubApi;
					var authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
					request.Headers["Authorization"] = "Basic " + authorization;

					request.ContentType = "application/json";
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
					using (var response = await request.GetResponseAsync() as HttpWebResponse)
					{
						if (response != null)
						{
							await ResponseStreamLogicAsync(response, hub, contentIdUrl, hubApi, recursive, startdate, onlyUpdated, libraryID: libraryId);
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Trace.TraceError($"Fetch Content error : {ex.Message}");
					return ContentModelList;
				}
			}
			else
			{
				System.Diagnostics.Trace.TraceError($"Hub Map not found ");
				return ContentModelList;
			}
			ItemsFetched.Add($"Sync Ended at {DateTime.Now}");
			return ContentModelList;
		}

		public async Task<ContentModel> FetchContentFromIdAsync(string id, string hub)
		{
			if (Constants.Constants.HubNameToId.ContainsKey(hub)
				&& Constants.Constants.HubToApi.ContainsKey(hub))
			{
				var hubId = Constants.Constants.HubNameToId[hub];
				var hubApi = Constants.Constants.HubToApi[hub];
				try
				{
					var baseUrl = Constants.Constants.Endpoints.Base.Replace("{hubId}", hubId);
					var contentIdUrl = $"{baseUrl}{ Constants.Constants.Endpoints.FetchContentById}/{id}";
					var request = WebRequest.Create(new Uri(contentIdUrl));
					request.Method = "GET";

					string credidentials = "AcousticAPIKey" + ":" + hubApi;
					var authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
					request.Headers["Authorization"] = "Basic " + authorization;

					request.ContentType = "application/json";
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
					using (var response = await request.GetResponseAsync() as HttpWebResponse)
					{
						if (response != null)
						{
							var responseStream = response.GetResponseStream();
							if (responseStream != null)
							{
								var reader = new StreamReader(responseStream, Encoding.Default);
								var responseAsString = reader.ReadToEnd();

								string[] items = responseAsString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
							}
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Trace.TraceError($"Fetch Content error : {ex.Message}");
				}
			}
			return null;
		}

		public List<AssetModel> FetchAssetsList()
		{
			var assetList = AssetModelList;

			return assetList;
		}

		private async Task FecthContentByIdAsync(string contentIdUrl, List<string> artifactIds, string hubApi, bool recursive, string startDate)
		{
			foreach (var id in artifactIds)
			{
				try
				{
					string itemUrl = $"{contentIdUrl}/{id}";

					var request = WebRequest.Create(new Uri(itemUrl));
					request.Method = "GET";

					//request.Credentials = new NetworkCredential("AcousticAPIKey", hupApi);

					string credidentials = "AcousticAPIKey" + ":" + hubApi;
					var authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
					request.Headers["Authorization"] = "Basic " + authorization;

					request.ContentType = "application/json";
					ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
					using (var response = await request.GetResponseAsync() as HttpWebResponse)
					{
						if (response != null)
						{
							var responseStream = response.GetResponseStream();
							if (responseStream != null)
							{
								var reader = new StreamReader(responseStream, Encoding.Default);
								var responseAsString = reader.ReadToEnd();

								var itemObj = JsonConvert.DeserializeObject<JsonObject>(responseAsString);
								var itemClassification = itemObj["classification"].ToString();
								var itemId = itemObj["id"];
								var itemName = $"{itemId}_cmd.json".Replace(":", "_");
								var lastModifiedDate = itemObj["lastModified"].ToString();

								if (ShouldUpdate(lastModifiedDate, startDate, recursive))
								{
									var msg = JsonCreator.CreateJsonFile(itemName, itemClassification, responseAsString);
									if (!string.IsNullOrWhiteSpace(msg))
									{
										ItemsFetched.Add(msg);
									}
								}

								await ExtractElementAsyncv2(itemObj, contentIdUrl, hubApi, recursive, startDate);

							}
						}
					}
				}
				catch (Exception ex)
				{
					var errorMsg = $"Fetch Content with id {id} error : {ex.Message} ";
					ItemsFetched.Add(errorMsg);

					System.Diagnostics.Trace.TraceError($"Fetch Content error : {ex.Message}");

				}
			}
		}


		public async Task<List<string>> FetchTypeAsync(int days, string hub, bool recursive, bool onlyUpdated)
		{
			ItemsFetched.Add($"Sync Started at {DateTime.Now}");
			int offset = 0;
			int itemReturned = 0;

			if (Constants.Constants.HubNameToId.ContainsKey(hub)
				&& Constants.Constants.HubToApi.ContainsKey(hub))
			{
				var hubId = Constants.Constants.HubNameToId[hub];
				var hubApi = Constants.Constants.HubToApi[hub];
				try
				{
					while (itemReturned >= 0)
					{
						var baseUrl = Constants.Constants.Endpoints.Base.Replace("{hubId}", hubId);
						var typeUrl = $"{baseUrl}{ Constants.Constants.Endpoints.FetchType}";

						var parameters = $"format=sequence&offset={offset}&limit=10";
						typeUrl = $"{typeUrl}?{parameters}";

						var request = WebRequest.Create(new Uri(typeUrl));
						request.Method = "GET";

						string credidentials = "AcousticAPIKey" + ":" + hubApi;
						var authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
						request.Headers["Authorization"] = "Basic " + authorization;

						request.ContentType = "application/json";
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
						using (var response = await request.GetResponseAsync() as HttpWebResponse)
						{
							if (response != null)
							{
								itemReturned = await ResponseStreamLogicTypeAsync(response, string.Empty, hubApi, recursive, string.Empty, onlyUpdated, itemReturned, true);
								offset = offset + itemReturned;
							}
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Trace.TraceError($"Fetch Content error : {ex.Message}");
					return ItemsFetched;
				}
			}
			else
			{
				System.Diagnostics.Trace.TraceError($"Hub Map not found ");
				return ItemsFetched;
			}
			ItemsFetched.Add($"Sync Ended at {DateTime.Now}");
			return ItemsFetched;
		}


		private async Task ResponseStreamLogicAsync(HttpWebResponse response,
			string hub, string contentIdUrl, string hubApi, bool recursive,
			string startdate, bool onlyUpdated, bool isAsset = false, string libraryID = null)
		{

			var responseStream = response.GetResponseStream();
			if (responseStream != null)
			{
				var reader = new StreamReader(responseStream, Encoding.Default);
				var responseAsString = reader.ReadToEnd();

				string[] items = responseAsString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				if (items != null && items.Any())
				{
					foreach (var item in items)
					{
						var itemObj = JsonConvert.DeserializeObject<JsonObject>(item);
						var itemClassification = itemObj["classification"].ToString();
						var itemId = itemObj["id"];
						var itemName = $"{itemId}_cmd.json".Replace(":", "_");
						var itemModifiedDate = itemObj["lastModified"].ToString();
						var libraryId = itemObj["libraryId"].ToString();
						if (!string.IsNullOrEmpty(libraryID) && libraryId != libraryID)
						{
							continue;
						}

						var msg = JsonCreator.CreateJsonFile(itemName, itemClassification, item);
						if (!string.IsNullOrWhiteSpace(msg))
						{
							ItemsFetched.Add(msg);

							var name = itemObj["name"].ToString();
							ContentModelList.Add(new ContentModel
							{
								ItemId = itemId.ToString(),
								ItemName = name,
								LibraryId = libraryId,
								LibraryName = Constants.Constants.LibraryIdMap[libraryId],
								Filename = itemName,
								ModifiedDate = itemModifiedDate
							});

						}

						//need to know onlyUpdated is defined
						if (!isAsset) //&& !onlyUpdated)
						{
							await ExtractElementAsyncv2(itemObj, contentIdUrl, hubApi, recursive, startdate);
							await GetAssetPath(hub);
						}
					}
				}
			}
		}
		private bool ShouldUpdate(string lastModifiedDate, string startDate, bool recursive)
		{
			DateTime outDate;
			if (DateTime.TryParse(startDate, out outDate) && DateTime.TryParse(lastModifiedDate, out outDate))
			{
				if (!recursive)
				{
					DateTime strtDate = DateTime.Parse(startDate).Date;
					DateTime modifiedDate = DateTime.Parse(lastModifiedDate).Date;
					if (modifiedDate > strtDate) return true;
					return false;

				}
			}
			return true;

		}
		private async Task ExtractElementAsync(JsonObject itemObj, string contentIdUrl, string hubApi, bool recursive, string startDate)
		{
			var elementString = itemObj["elements"].ToString();
			var itemId = itemObj["id"].ToString();
			List<string> associatedId = new List<string>();

			string[] elements = elementString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			if (elements != null && elements.Any())
			{
				foreach (var element in elements)
				{
					if (element.Contains("\"id\":"))
					{
						var stringSplit = element.Trim().Split(' ');
						if (stringSplit.Length > 1)
						{
							var id = stringSplit[1].Replace("\"", "");
							if (!associatedId.Contains(id) && !string.Equals(itemId, id))
							{
								associatedId.Add(id);
							}
						}
					}
				}
			}

			if (associatedId.Any())
			{
				await FecthContentByIdAsync(contentIdUrl, associatedId, hubApi, recursive, startDate);
			}

		}




		private async Task ExtractElementAsyncv2(JsonObject itemObj, string contentIdUrl, string hubApi, bool recursive, string startDate)
		{
			var elementString = itemObj["elements"].ToString();
			List<string> associatedId = new List<string>();
			var itemId = itemObj["id"].ToString();

			if (!elementString.StartsWith("{"))
			{
				elementString = string.Concat("{", elementString, "}");
			}

			var elementList = JsonConvert.DeserializeObject<Dictionary<string, object>>(elementString);
			if (elementList != null && elementList.Any())
			{
				foreach (var element in elementList)
				{
					var elementValue = element.Value.ToString();
					if (elementValue.Contains(" \"elementType\": \"reference\""))
					{
						//find reference
						string[] elements = elementValue.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						if (elements != null && elements.Any())
						{
							for (int i = 0; i < elements.Length; i++)
							{
								var ele = elements[i];
								if (ele.Contains("\"id\":") && CheckPrevious(i, elements))
								{
									var stringSplit = ele.Trim().Split(' ');
									if (stringSplit.Length > 1)
									{
										var id = stringSplit[1].Replace("\"", "");
										if (!associatedId.Contains(id) && !string.Equals(itemId, id))
										{
											associatedId.Add(id);
										}
									}
								}
							}
						}
					}

					else if (elementValue.Contains(" \"elementType\": \"image\"") || elementValue.Contains(" \"elementType\": \"video\""))
					{
						string[] elements = elementValue.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						if (elements != null && elements.Any())
						{
							for (int i = 0; i < elements.Length; i++)
							{
								var ele = elements[i];
								if (ele.Contains("\"id\":") && CheckPrevious(i, elements))
								{
									var stringSplit = ele.Trim().Split(' ');
									if (stringSplit.Length > 1)
									{
										var id = stringSplit[1].Replace("\"", "");
										if (!AssociatedAssetsId.Contains(id) && !string.Equals(itemId, id))
										{
											AssociatedAssetsId.Add(id);
										}
									}
								}
							}
						}
					}
				}
			}

			if (associatedId.Any())
			{
				await FecthContentByIdAsync(contentIdUrl, associatedId, hubApi, recursive, startDate);
			}

		}


		private bool CheckPrevious(int index, string[] elementList)
		{
			if (index > 0)
			{
				var previousElement = elementList[index - 1];
				if (previousElement.Contains("\"typeRef\":"))
				{
					return false;
				}
			}
			return true;

		}

		private bool IsAsset(int index, string[] elementList)
		{
			if (index > 0)
			{
				var previousElement = elementList[index - 1];
				if (previousElement.Contains("\"asset\":"))
				{
					return true;
				}
			}

			return false;
		}


		private async Task<int> ResponseStreamLogicTypeAsync(HttpWebResponse response,
		   string contentIdUrl, string hubApi, bool recursive,
		   string startdate, bool onlyUpdated, int itemCount, bool isAsset = false)
		{

			var responseStream = response.GetResponseStream();
			if (responseStream != null)
			{
				var reader = new StreamReader(responseStream, Encoding.Default);
				var responseAsString = reader.ReadToEnd();

				string[] items = responseAsString.Split('\n');

				itemCount = items.Length;

				if (items != null && items.Any())
				{
					foreach (var item in items)
					{
						//var itemObj = item;
						var itemObj = JsonConvert.DeserializeObject<JsonObject>(item);
						var itemClassification = itemObj["classification"].ToString();
						var itemId = itemObj["name"];
						var itemName = $"{itemId}.json".Replace(":", "_").Replace(" ", "-");

						var msg = JsonCreator.CreateJsonFile(itemName, "types", item);
						if (!string.IsNullOrWhiteSpace(msg))
						{
							ItemsFetched.Add(msg);
							var libraryId = itemObj["libraryId"].ToString();
							var name = itemObj["name"].ToString();
							ContentModelList.Add(new ContentModel
							{
								ItemId = itemId.ToString(),
								ItemName = name,
								LibraryId = libraryId,
								LibraryName = Constants.Constants.LibraryIdMap[libraryId],
								Filename = itemName
							});

						}

						if (!isAsset && !onlyUpdated)
						{
							//await ExtractElementAsync(itemObj, contentIdUrl, hubApi, recursive, startdate);

							await ExtractElementAsyncv2(itemObj, contentIdUrl, hubApi, recursive, startdate);

						}
					}
				}
			}
			else
			{
				itemCount = -1;
			}
			if (itemCount == 0)
			{
				itemCount = -1;
			}
			return itemCount;
		}


		public async Task<List<ContentModel>> FetchContentByLibrary(string hub, string libraryId)
		{
			ItemsFetched.Add($"Sync Started at {DateTime.Now}");
			int offset = 0;
			int itemReturned = 0;

			if (Constants.Constants.HubNameToId.ContainsKey(hub)
				&& Constants.Constants.HubToApi.ContainsKey(hub))
			{
				var hubId = Constants.Constants.HubNameToId[hub];
				var hubApi = Constants.Constants.HubToApi[hub];
				try
				{
					while (itemReturned >= 0)
					{
						var baseUrl = Constants.Constants.Endpoints.Base.Replace("{hubId}", hubId);
						var typeUrl = $"{baseUrl}{ Constants.Constants.Endpoints.FetchContentById}";

						var parameters = $"offset={offset}&limit=50&format=sequence";
						typeUrl = $"{typeUrl}?{parameters}";

						var request = WebRequest.Create(new Uri(typeUrl));
						request.Method = "GET";

						string credidentials = "AcousticAPIKey" + ":" + hubApi;
						var authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
						request.Headers["Authorization"] = "Basic " + authorization;

						request.ContentType = "application/json";
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
						using (var response = await request.GetResponseAsync() as HttpWebResponse)
						{
							if (response != null && response.StatusCode == HttpStatusCode.OK)
							{
								itemReturned = ResponseStreamLogicContentAsync(response, itemReturned, libraryId);
								offset = offset + itemReturned;
							}
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Trace.TraceError($"Fetch Content error : {ex.Message}");
					return ContentModelList;
				}
			}
			else
			{
				System.Diagnostics.Trace.TraceError($"Hub Map not found ");
				return ContentModelList;
			}
			ItemsFetched.Add($"Sync Ended at {DateTime.Now}");
			return ContentModelList;
		}

		/// <summary>
		/// Fetches modified content by library
		/// </summary>
		/// <param name="startdate">start date</param>
		/// <param name="hub">hub name</param>
		/// <param name="libraryId">library id</param>
		/// <returns>list of <see cref="ContentModel"></returns>
		public async Task<List<ContentModel>> FetchModifiedContentByLibrary(DateTime startdate, string hub, string libraryId)
		{
			var modifiedContent = await FetchArtifactForDateRangeAsync(startdate, hub, true, true);

			return modifiedContent.Where(x => x.LibraryId == libraryId).ToList();
		}

		public async Task<List<AssetModel>> GetAssetPath(string sourceHub)
		{
			foreach (var assetId in AssociatedAssetsId)
			{
				if (Constants.Constants.HubNameToId.ContainsKey(sourceHub) &&
					Constants.Constants.HubToApi.ContainsKey(sourceHub))
				{

					var hubId = Constants.Constants.HubNameToId[sourceHub];
					var hubApi = Constants.Constants.HubToApi[sourceHub];

					try
					{
						var baseUrl = Constants.Constants.Endpoints.Base.Replace("{hubId}", hubId);
						var assetUrl = $"{baseUrl}{Constants.Constants.Endpoints.FetchAssetsById}/{assetId}";

						var parameters = "fields=path";
						assetUrl = $"{assetUrl}?{parameters}";

						var request = WebRequest.Create(new Uri(assetUrl));
						request.Method = "GET";

						string credidentials = "AcousticAPIKey" + ":" + hubApi;
						var authorization = Convert.ToBase64String(Encoding.Default.GetBytes(credidentials));
						request.Headers["Authorization"] = "Basic " + authorization;

						request.ContentType = "application/json";
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
						using (var response = await request.GetResponseAsync() as HttpWebResponse)
						{
							if (response != null && response.StatusCode == HttpStatusCode.OK)
							{
								AssetModel asset = new AssetModel
								{
									Path = ResponseStreamAssetPath(response)
								};

								AssetModelList.Add(asset);
							}
						}
					}

					catch (Exception ex)
					{
						System.Diagnostics.Trace.TraceError($"Fetch Content error : {ex.Message}");
					}
				}
			}

			return AssetModelList;
		}

		private int ResponseStreamLogicContentAsync(HttpWebResponse response,
		  int itemCount, string libraryId)
		{

			var responseStream = response.GetResponseStream();
			if (responseStream != null && response.StatusCode == HttpStatusCode.OK)
			{
				var reader = new StreamReader(responseStream, Encoding.Default);
				var responseAsString = reader.ReadToEnd();
				if (!string.IsNullOrEmpty(responseAsString))
				{
					string[] items = responseAsString.Split('\n');
					itemCount = items.Length;

					if (items != null && items.Any())
					{
						foreach (var item in items)
						{
							if (!string.IsNullOrEmpty(item))
							{
								//var itemObj = item;
								try
								{
									var itemObj = JsonConvert.DeserializeObject<JsonObject>(item);

									if (itemObj.ContainsKey("libraryId"))
									{
										var itemId = itemObj["id"];
										var itemLibraryId = itemObj["libraryId"].ToString();
										if (itemLibraryId == libraryId || itemLibraryId == "default")
										{
											var itemClassification = itemObj["classification"].ToString();

											var itemName = $"{itemId}_cmd.json".Replace(":", "_sep_").Replace(" ", "-");

											var msg = JsonCreator.CreateJsonFile(itemName, itemClassification, item);
											if (!string.IsNullOrWhiteSpace(msg))
											{
												ItemsFetched.Add(msg);
												var name = itemObj["name"].ToString();
												ContentModelList.Add(new ContentModel
												{
													ItemId = itemId.ToString(),
													ItemName = name,
													LibraryId = itemLibraryId,
													LibraryName = Constants.Constants.LibraryIdMap[itemLibraryId],
													Filename = itemName
												});
											}
										}
									}
								}
								catch (Exception ex)
								{

									var errorMsg = $"Fetch Content {item} error : {ex.Message} ";
									ItemsFetched.Add(errorMsg);
								}
							}
						}
					}
				}
				else
				{
					itemCount = -1;
				}
			}
			else
			{
				itemCount = -1;
			}
			if (itemCount == 0)
			{
				itemCount = -1;
			}
			return itemCount;
		}

		private string ResponseStreamAssetPath(HttpWebResponse response)
		{
			string assetPath = string.Empty;
			var responseStream = response.GetResponseStream();
			if (responseStream != null && response.StatusCode == HttpStatusCode.OK)
			{
				var reader = new StreamReader(responseStream, Encoding.Default);
				var responseAsString = reader.ReadToEnd();
				if (!string.IsNullOrEmpty(responseAsString))
				{
					dynamic data = JObject.Parse(responseAsString);
					assetPath = data.path;
				}
			}
			return assetPath;
		}
	}
}
