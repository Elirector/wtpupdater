﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        private void AddLog(string message)
        {
            ActionLog.AppendText($"{DateTime.Now.ToLongTimeString()}\t{message}{Environment.NewLine}");
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            AddLog($"Getting release page from {GH_URI}");
            string result;
            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(GH_URI))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        result = sr.ReadToEnd();
                    }
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
    }
}