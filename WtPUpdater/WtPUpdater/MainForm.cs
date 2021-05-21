using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WtPUpdater
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ServicePointManager.SecurityProtocol =  (SecurityProtocolType)3072;
        }

        private const string GH_URI = "https://github.com/We-the-People-civ4col-mod/Mod/releases";

        private Dictionary<string,string> VersionUris;
        private WebClient WebClient = new WebClient();

        private void AddLog(string message)
        {
            ActionLog.AppendText($"{DateTime.Now.ToLongTimeString()}\t{message}{Environment.NewLine}");
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            AddLog($"Getting release page from {GH_URI}");
            string result;
           
                using (var stream = WebClient.OpenRead(GH_URI))
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
                VersionList.Items.Add(version);
                var verUri = $"{GH_URI}/download/{match.Groups["suburl"].Value}";
                AddLog($"Found version {version} at {verUri}");
                VersionUris.Add(version, verUri);
            }

            VersionList.SelectedIndex = 0;
        }

        private void VersionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveButton.Enabled = VersionList.SelectedIndex >= 0;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (WebClient.IsBusy) {WebClient.CancelAsync();
                SaveButton.Text = "Save";
                return;
            }

            SaveButton.Text = "Cancel";
            var dir = Path.GetTempPath();
            var uri = new Uri(VersionUris[(string)VersionList.SelectedItem]);
            var fileName = Path.Combine(dir, uri.AbsolutePath);
            fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

            WebClient.DownloadProgressChanged += (s, ev) =>
            {
                progressBar.Value = ev.ProgressPercentage;
            };
            WebClient.DownloadFileCompleted += (s, ev) =>
            {
                progressBar.Visible = false;
                string argument = "/select, \"" + fileName + "\"";
                Process.Start("explorer.exe", argument);
                SaveButton.Text = "Save";
            };
            
            
            WebClient.DownloadFileAsync(uri,
                fileName);
           

        }

        private string installDir = "";
        private void FindCivButton_Click(object sender, EventArgs e)
        {
            installDir = "";
           var uninst =  Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
           foreach (var subkeyname in uninst.GetSubKeyNames())
            {
                var subkey = uninst.OpenSubKey(subkeyname);
                if (subkey?.GetValue("DisplayName")?.ToString() != "Sid Meier's Civilization IV: Colonization1") continue;
               installDir = subkey.GetValue("InstallLocation").ToString();
            }
           if (string.IsNullOrEmpty(installDir))
            {
                if (MessageBox.Show("Civilization IV: Colonization install dir not found, do you like to select it manually?", "Find directory", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var fod = new OpenFileDialog() { Filter = "Colonization.exe|Colonization.exe" };
                    if (fod.ShowDialog() == DialogResult.OK && File.Exists(fod.FileName))
                    {
                        installDir = Path.GetDirectoryName(fod.FileName);
                    }
                }
                else
                {
                    var mgp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Civilization IV Colonization");
                    if (Directory.Exists(mgp))
                    {
                        if (MessageBox.Show($"Do you want to use {mgp}?") == DialogResult.OK) installDir = mgp;
                    }
                }
            }
           if (!string.IsNullOrEmpty(installDir)&&Directory.Exists(installDir))
            {
                installDir = Path.Combine(installDir, "Mods");
                AddLog($"Installation dir: {installDir}");
            } 
              
        }
    }
}
