using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KKIHUB.ContentSync.Web.Helper
{
    public static class CommandHelper
    {
        public static void ExcecuteScript(string filePath, string syncId, string assetList)
        {
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -File \"{filePath}\" {syncId}",
                    UseShellExecute = true
                };

                startInfo.Arguments = assetList != null
                   ? $"-NoProfile -ExecutionPolicy unrestricted -File \"{filePath}\" {syncId} {assetList}"
                   : startInfo.Arguments;

                Process.Start(startInfo);
            }

            catch (Exception err)
            {
                Debug.WriteLine(err);
            }
        }

        public static string ExcecuteScriptOutput(string filePath, string syncId)
        {
            var finalOutputMessage = string.Empty;
            filePath = Path.Combine(filePath, syncId);
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy unrestricted -File \"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                var process = Process.Start(startInfo);

                while (!process.StandardOutput.EndOfStream)
                {
                    finalOutputMessage = process.StandardOutput.ReadLine();
                }
            }

            catch (Exception err)
            {
                Debug.WriteLine(err);
                finalOutputMessage = err.ToString();
            }

            return finalOutputMessage;
        }
    }
}
