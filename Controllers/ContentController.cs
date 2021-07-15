using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using KKIHUB.ContentSync.Web.Helper;
using KKIHUB.ContentSync.Web.Model;
using KKIHUB.ContentSync.Web.Service;
using Newtonsoft.Json;

namespace KKIHUB.ContentSync.Web.Controllers
{
    //[Route("api/[controller]")]
    public class ContentController : Controller
    {
        private IContentService ContentService { get; set; }

        public ContentController() : this(new ContentService(new AcousticService()))
        {

        }
        public ContentController(IContentService contentService)
        {
            this.ContentService = contentService;
        }


        [HttpGet]
        [Route("SyncComplete")]
        public async Task<ActionResult> SyncContentAsync(DateTime startDate, DateTime endDate, string sourceHub, string targetHub, string syncId)
        {
            var content = await ContentService.FetchContentAsync(syncId, startDate, endDate, sourceHub, false, false);

            return Json(content);
        }


        [HttpGet]
        [Route("SyncRecursive")]
        public async Task<ActionResult> SyncContentRecursive(DateTime startDate, DateTime endDate, string sourceHub, string targetHub, string syncId)
        {
            var content = await ContentService.FetchContentAsync(syncId, startDate, endDate, sourceHub, true, false);

            return Json(content);
        }

        [HttpGet]
        [Route("SyncUpdated")]
        public async Task<ActionResult> SyncContentUpdated(PullModel pullModel)
        {
            List<ContentModel> content = new List<ContentModel>();
            var fetchModel = new FetchContentModel(pullModel);
            if (string.IsNullOrEmpty(pullModel.Library))
            {

                content = await ContentService.FetchContentAsync(fetchModel);
            }
            else
            {
                content = await ContentService.FetchModifiedContentByLibrary(fetchModel);
            }

            var idsList = new List<string>();
            foreach (var item in content)
            {
                if (item.ReferencedItemIds.Count() > 0)
                {
                    idsList = idsList.Concat(item.ReferencedItemIds).ToList();
                }
            }

            var parentList = new List<ContentModel>();

            if (idsList.Any())
            {
                foreach (var currentItem in content)
                {
                    if (!idsList.Contains(currentItem.ItemId))
                    {
                        parentList.Add(currentItem);
                    }
                }
            }
            else
            {
                parentList = content;
            }

            //var itemsHavingReferences = content.Where(i => i.ReferencedItemIds.Count > 0);
            var remainingItems = content.Except(parentList);

            var finalList = parentList.OrderBy(x => x.ItemName).Concat(remainingItems);
            return Json(finalList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Route("ContentByLibrary")]
        public async Task<ActionResult> ContentByLibrary(string sourceHub, string libraryId, string syncId)
        {
            var content = await ContentService.FetchContentByLibrary(syncId, sourceHub, libraryId);

            return Json(content);
        }

        [HttpGet]
        [Route("PushContent")]
        public JsonResult PushContent(string filepaths, string syncId)
        {
            string message = string.Empty;
            if (!string.IsNullOrEmpty(filepaths))
            {
                try
                {
                    //Dictionary<string, string> pushData = JsonConvert.DeserializeObject<Dictionary<string, string>>(pushParams);
                    var files = filepaths;
                    //var targetHub = pushData["targethub"];

                    if (!string.IsNullOrEmpty(files))
                    {
                        var filePath = files.Split('|').ToList();

                        var contentList = JsonCreator.ListContent(syncId, "content");
                        if (contentList.Any() && filePath.Any())
                        {
                            var itemsToDelete = contentList.Except(filePath).ToList();
                            if (itemsToDelete.Any())
                            {
                                var flag = JsonCreator.Delete(syncId, "content", itemsToDelete);
                            }
                            //message = CommandHelper.ExcecuteScriptOutput(Path.Combine(Environment.CurrentDirectory, Constants.Constants.Path.WchtoolsPath));
                            var assets = ContentService.FetchAssetsList().ToList();

                            var assetString = string.Empty;
                            foreach (var asset in assets)
                            {
                                assetString = string.Join("|", asset.Path);
                            }

                            assetString = assetString.TrimEnd('|');
                            CommandHelper.ExcecuteScript(Path.Combine(HttpRuntime.AppDomainAppPath, Constants.Constants.Path.WchtoolsPath), syncId, assetString);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError($"Error pushing content at {ex.Message} ");
                    message = ex.Message;
                }
            }

            //var content = await ContentService.FetchContentByLibrary(sourceHub, libraryId);

            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route("PushContentv2")]
        public JsonResult PushContentv2(string pushParams)
        {
            string msg = string.Empty;
            List<string> output = new List<string>();
            var pushAssetFlag = false;
            var cleanUpAssetFolder = false;
            var listOfAssetsIds = new List<string>();
            var pathDynamic = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(pushParams))
            {
                PushParams pushData = JsonConvert.DeserializeObject<PushParams>(pushParams);
                if (pushData != null)
                {
                    var hubId = Constants.Constants.HubNameToId[pushData.SourceHub];
                    var syncId = pushData.SyncId;
                    if (pushData.ContentDetails != null && pushData.ContentDetails.Any())
                    {
                        DeleteUnnecessaryItems(pushData.ContentDetails, syncId);
                    }

                    var assetsFiles = pushData.ContentDetails.Select(i => i.Assets).ToList();
                    if (assetsFiles != null && assetsFiles.Any())
                    {
                        foreach (var assetFile in assetsFiles)
                        {
                            if (assetFile != null && assetFile.Any())
                            {
                                foreach (var asset in assetFile)
                                {
                                    if (!string.IsNullOrEmpty(asset))
                                    {
                                        pushAssetFlag = true;
                                        var assetPath = asset;
                                        if (asset.Contains(" "))
                                        {
                                            var lastIndex = asset.LastIndexOf("/");
                                            assetPath = asset.Substring(0, lastIndex);
                                            cleanUpAssetFolder = true;
                                            var assetId = assetPath.Substring(assetPath.LastIndexOf("/") + 1);
                                            listOfAssetsIds.Add(assetId);
                                        }
                                        var assetMsg = PullAssets(hubId, assetPath, syncId);
                                        msg = msg + " " + assetMsg;
                                    }
                                }
                            }
                        }
                    }

                    if (cleanUpAssetFolder)
                    {
                        DeleteUnnecessaryAssets(listOfAssetsIds, syncId);
                    }

                    //push asset
                    if (pushAssetFlag)
                    {
                        var innerMsg = PushCommand(pushData.TargetHub, syncId, "pushasset");
                        output.AddRange(CleanOutput(innerMsg, "= Pushing content assets ="));
                    }


                    //push content
                    var innerMsg2 = PushCommand(pushData.TargetHub, syncId, "pushcontent");
                    output.AddRange(CleanOutput(innerMsg2, "= Pushing content ="));


                    //DeleteAllFiles(syncId);
                }
            }
            return Json(output);
        }



        private List<string> CleanOutput(List<string> list, string type)
        {
            List<string> outputMsg = new List<string>();
            var index = 0;
            if (list.Any())
            {
                foreach (var entry in list)
                {
                    if (entry.Contains(type))
                    {
                        index = list.IndexOf(entry);
                        break;
                    }
                }

                if (index > 0)
                {
                    var length = list.Count();

                    while (index < length)
                    {
                        var strOut = list[index];
                        outputMsg.Add(strOut);
                        index++;

                        if (strOut.StartsWith("Push complete") || strOut.StartsWith("No items to be pushed"))
                        {
                            break;
                        }
                    }
                }
            }
            return outputMsg;
        }

        public List<string> PushCommand(string targetHub, string syncId, string jobName)
        {
            var targetHubId = Constants.Constants.HubNameToId[targetHub];
            var path = HttpRuntime.AppDomainAppPath;
            string initCommand = $"/C npm run {jobName} -- --syncid {syncId} --hubid {targetHubId}";
            List<string> output = new List<string>();
            string msg = string.Empty;
            try
            {
                var p = new Process
                {
                    StartInfo =
                             {
                                 FileName = "cmd.exe",
                                 WorkingDirectory = path,
                                 Arguments = initCommand,
                                 UseShellExecute = false,
                                 RedirectStandardOutput = true,
                                 Verb= "runas"
                            }
                };
                p.Start();
                while (!p.StandardOutput.EndOfStream)
                {
                    output.Add(p.StandardOutput.ReadLine());
                }
                //msg = p.StandardOutput.ReadToEnd();
            }
            catch (Exception err)
            {
                msg = $"{err.Message} at {err.StackTrace}";
                output.Add(msg);
            }
            return output;
        }

        private string PullAssets(string sourceHub, string assetPath, string syncId)
        {
            var path = HttpRuntime.AppDomainAppPath;
            string initCommand = $"/C npm run pullasset -- --path \"{assetPath}\"  --syncid {syncId} --hubid {sourceHub}";
            string msg = string.Empty;
            try
            {
                var p = new Process
                {
                    StartInfo =
                     {
                         FileName = "cmd.exe",
                         WorkingDirectory = path,
                         Arguments = initCommand,
                         UseShellExecute = false,
                         RedirectStandardOutput = true,
                         Verb= "runas"
                    }
                };
                p.Start();
                msg = p.StandardOutput.ReadToEnd();
            }

            catch (Exception err)
            {
                msg = $"{err.Message} at {err.StackTrace}";
            }

            return msg;
        }


        private void DeleteUnnecessaryItems(List<ContentDetails> contents, string syncId)
        {
            var contentList = JsonCreator.ListContent(syncId, "content");
            var filePaths = contents.Select(i => i.Item).ToList();

            var itemsToDelete = contentList.Except(filePaths).ToList();
            if (itemsToDelete.Any())
            {
                var flag = JsonCreator.Delete(syncId, "content", itemsToDelete);
            }
        }


        private void DeleteUnnecessaryAssets(List<string> contents, string syncId)
        {
            var assetFolders = JsonCreator.ListDirectories(syncId, "assets/dxdam");
            if (assetFolders != null)
            {
                //aa
                foreach (var directory in assetFolders)
                {
                    //aaflkn-dfsf
                    var innerAssetFolders = JsonCreator.ListDirectories(syncId, "assets/dxdam/" + directory);
                    var foldersToDelete = innerAssetFolders.Except(contents).ToList();
                    if (foldersToDelete.Any())
                    {
                        JsonCreator.DeleteFolder(syncId, "assets", foldersToDelete);
                    }
                }
            }

        }

        private void DeleteAllFiles(string syncId)
        {
            var flag = JsonCreator.DeleteAll(syncId);

        }

        private void ExecuteCommand()
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, Constants.Constants.Path.ArtifactPath, "wchtools_non-prod.cmd");
            //System.Diagnostics.Process.Start(filePath);


            var process = new Process();
            //var startinfo = new ProcessStartInfo(filePath, "\"1st_arg\" \"2nd_arg\" \"3rd_arg\"");
            var startinfo = new ProcessStartInfo("cmd.exe", "/c " + filePath);
            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            process.StartInfo = startinfo;
            process.OutputDataReceived += (sender, argsx) => Console.WriteLine(argsx.Data); // do whatever processing you need to do in this handler
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }


        static void ExecuteCommandInApp(string command, string workingDirectory)
        {
            int exitCode;

            var nodePath = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Microsoft\VisualStudio\NodeJs\win-x64\node.exe";
            string initCommand = "wchtools init --url https://content-eu-1.content-cms.com/api/37dd7bf6-5628-4aac-8464-f4894ddfb8c4 --user adarsh.bhautoo@hangarww.com --password Ad1108bh_hangarMU";

            var processInfo = new ProcessStartInfo(nodePath, "/c " + initCommand);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.WorkingDirectory = workingDirectory;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }



        private void SyncContent()
        {

            var filePath = Path.Combine(Environment.CurrentDirectory, Constants.Constants.Path.ArtifactPath);

            //string installWchtools = "call npx -i -g --production --no-optional wchtools-cli";
            // ExecuteCommandInApp(installWchtools, filePath);
            //string initCommand = "npx wchtools init --url https://content-eu-1.content-cms.com/api/37dd7bf6-5628-4aac-8464-f4894ddfb8c4 --user adarsh.bhautoo@hangarww.com --password Ad1108bh_hangarMU";
            //ExecuteCommandInApp(initCommand, filePath);

            var commandFile = string.Concat(filePath, "wchtools_non-prod.cmd");
            ExecuteCommandInApp(commandFile, filePath);

        }
    }
}