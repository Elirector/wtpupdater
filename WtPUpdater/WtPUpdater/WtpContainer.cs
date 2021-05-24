using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace WtPUpdater
{
    class WtpContainer
    {
        private readonly WebClient _webClient;
        private readonly Action<string> _addLog;
        private Dictionary<string, string> _versionUris;
        private readonly string _ghUri;
        private readonly List<string> _tempFiles = new List<string>();

        internal DownloadProgressChangedEventHandler DownloadProgressChanged;
        internal AsyncCompletedEventHandler DownloadFileCompleted;
        internal string WtpZipFile { get; private set; }
        internal bool DownloadInProgress => _webClient?.IsBusy ?? true;


        public WtpContainer(string ghUri, Action<string> addLog = null)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            _addLog = addLog ?? ((s) => { });
            _webClient = new WebClient();
            _ghUri = ghUri;
        }

        internal IEnumerable<string> GetVerList()
        {
            var versionList = new List<string>();
            _addLog($"Getting release page from {_ghUri}");

            try
            {
                string result;
                using (var stream = _webClient.OpenRead(_ghUri))
                {
                    using (var sr = new StreamReader(stream ?? throw new InvalidOperationException("Can not connect to GitHub")))
                    {
                        result = sr.ReadToEnd();
                    }
                }
                _addLog($"Got {result.Length} chars");
                _versionUris = new Dictionary<string, string>();
                var re = new Regex(@"^.*/releases/download/(?<suburl>.*?/WeThePeople-(?<version>\d*?[.]\d*?([.]\d*?([.]\d*?))?).zip).*$", RegexOptions.Multiline);
                var matches = re.Matches(result);
                foreach (Match match in matches)
                {

                    var version = match.Groups["version"].Value;
                    versionList.Add(version);
                    var verUri = $"{_ghUri}/download/{match.Groups["suburl"].Value}";
                    _addLog($"Found version {version} at {verUri}");
                    _versionUris.Add(version, verUri);
                }
            }
            catch (Exception e)
            {
                _addLog(e.ToString());

            }
            return versionList;

        }


        internal void Download(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return; //no version selected, ignoring
            }
            var dir = Path.GetTempPath();
            Uri uri;
            try
            {
                uri = new Uri(_versionUris[version]);
            }
            catch (KeyNotFoundException)
            {
                _addLog($"Uri for ver. {version} not found in collection");
                return;
            }
            var fullName = Path.Combine(dir, Path.GetFileName(uri.AbsolutePath));
            if (File.Exists(fullName))
            {
                _addLog($"File already exists: {fullName}, removing");
                try
                {
                    File.Delete(fullName);
                }
                catch (Exception)
                {
                    _addLog("Can not remove file, cancelling");
                    return;
                }
            }
            _webClient.DownloadProgressChanged += (o, ea) => DownloadProgressChanged?.Invoke(o, ea);
            _webClient.DownloadFileCompleted += (o, ea) => DownloadFileCompleted?.Invoke(o, ea);
            _webClient.DownloadFileAsync(uri, fullName);
            _tempFiles.Add(fullName);
            try
            {
                if (!string.IsNullOrEmpty(WtpZipFile) && File.Exists(WtpZipFile)) File.Delete(WtpZipFile);
            }
            catch (Exception)
            {
                _tempFiles.Add(WtpZipFile);
                _addLog("Can not delete old downloaded file, will try to remove it later");

            }
            WtpZipFile = fullName;
        }

        internal string FindCiv4ColDir()
        {
            try
            {
                var uninstall = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (uninstall==null)
                {
                    _addLog("Can not get registry key");
                    return "";
                }
                foreach (var subKeyName in uninstall.GetSubKeyNames())
                {
                    var subKey = uninstall.OpenSubKey(subKeyName);
                    if (subKey?.GetValue("DisplayName")?.ToString() != "Sid Meier's Civilization IV: Colonization") continue;
                    return subKey.GetValue("InstallLocation").ToString();
                }
            }
            catch (Exception e)
            {
                _addLog($"Error searching for installation dir: {e}");
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
            try
            {
                if (_webClient.IsBusy)
                {
                    _webClient.CancelAsync();
                    _addLog("Download cancelled");
                }
            }
            catch (Exception)
            {
                _addLog("Can not cancel file download");
            }

            try
            {
                if (string.IsNullOrEmpty(WtpZipFile) || !File.Exists(WtpZipFile)) return;
                File.Delete(WtpZipFile);
                _addLog($"Deleted uncompleted {WtpZipFile}");
                WtpZipFile = "";
            }
            catch (Exception e)
            {
                _addLog($"Can not delete uncompleted file: {e}");
            }
        }

        internal bool Unzip(string installPath)
        {
            try
            {
                var path = Path.Combine(installPath, "WeThePeople");
                _addLog($"Target directory {path}");
                if (Directory.Exists(path))
                {
                    _addLog("Directory exists, removing");
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
                _addLog("Unzipping");
                ZipFile.ExtractToDirectory(WtpZipFile, path);
                _addLog("Completed");
                return true;
            }
            catch (Exception ex)
            {
                _addLog($"Error in unzip: {ex}");
                return false;
            }
        }

        internal void RemoveFile()
        {
            if (!File.Exists(WtpZipFile)) return;
            _addLog($"Removing {WtpZipFile}");
            try
            {
                File.Delete(WtpZipFile);
                _addLog("Completed!");
            }
            catch (Exception e)
            {
                _addLog($"Can not remove file after unzipping: {e}");
            }
        }

        ~WtpContainer()
        {
            foreach (var file in _tempFiles.Where(File.Exists))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    //Just ignore undeletable file, can do nothing now
                }


            }
        }

    }
}
