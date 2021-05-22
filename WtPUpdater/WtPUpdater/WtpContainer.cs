using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WtPUpdater
{
    class WtpContainer
    {
        private WebClient WebClient;
        private Action<string> AddLog = null;
        private Dictionary<string, string> VersionUris;
        private readonly string GhUri;
        internal DownloadProgressChangedEventHandler DownloadProgressChanged;
        internal AsyncCompletedEventHandler DownloadFileCompleted;

        internal bool DownloadInProgress { get { return WebClient == null ? true : WebClient.IsBusy; } }
        private void TriggerDownloadProgressChanged(object o, DownloadProgressChangedEventArgs args)
        {
            if (DownloadProgressChanged != null) DownloadProgressChanged(o, args);
        }

        private void TriggerDownloadFileCompleted(object o, AsyncCompletedEventArgs args)
        {
            if (DownloadFileCompleted != null) DownloadFileCompleted(o, args);
        }
        public WtpContainer(Action<string> addLog = null, string ghUri = @"https://github.com/We-the-People-civ4col-mod/Mod/releases")
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            AddLog = addLog ?? ((s) => { });
             WebClient = new WebClient();
            GhUri = ghUri;
        }

        internal IEnumerable<string> GetVerList()
        {
            var versionList = new List<string>();
            AddLog($"Getting release page from {GhUri}");
            string result;

            using (var stream = WebClient.OpenRead(GhUri))
            {
                using (var sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            AddLog($"Got {result.Length} chars");
            VersionUris = new Dictionary<string, string>();
            var re = new Regex(@"^.*/releases/download/(?<suburl>.*?/WeThePeople-(?<version>\d*?[.]\d*?([.]\d*?([.]\d*?))?).zip).*$", RegexOptions.Multiline);
            var matches = re.Matches(result);
            foreach (Match match in matches)
            {

                var version = match.Groups["version"].Value;
                versionList.Add(version);
                var verUri = $"{GhUri}/download/{match.Groups["suburl"].Value}";
                AddLog($"Found version {version} at {verUri}");
                VersionUris.Add(version, verUri);
            }
            return versionList;

        }

        internal void Download(string version)
        {
            var dir = Path.GetTempPath();
            var uri = new Uri(VersionUris[version]);
            var fileName = Path.Combine(dir, uri.AbsolutePath);
            fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
            WebClient.DownloadProgressChanged += (o, ea) => TriggerDownloadProgressChanged(o, ea);
            WebClient.DownloadFileCompleted += (o, ea) => TriggerDownloadFileCompleted(o, ea);
            WebClient.DownloadFileAsync(uri, fileName);
        }

        internal string FindCiv4ColDir()
        {
            var uninst = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (var subkeyname in uninst.GetSubKeyNames())
            {
                var subkey = uninst.OpenSubKey(subkeyname);
                if (subkey?.GetValue("DisplayName")?.ToString() != "Sid Meier's Civilization IV: Colonization1") continue;
                return  subkey.GetValue("InstallLocation").ToString();
            }
            return "";
        }

        internal string FindCiv4ColMyGamesDir()
        {
            var mgp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Civilization IV Colonization");
            return Directory.Exists(mgp) ? mgp : "";
        }

        internal void CancelDownload()
        {
            if (WebClient.IsBusy) WebClient.CancelAsync();
        }

    }
}
