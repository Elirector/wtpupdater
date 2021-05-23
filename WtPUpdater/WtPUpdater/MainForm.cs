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

        private WtpContainer wtpContainer;
        public MainForm()
        {
            InitializeComponent();
           
            wtpContainer = new WtpContainer(AddLog, GH_URI);
            wtpContainer.DownloadFileCompleted += (o, arg) =>
            {
                UnzipButton.Enabled = !string.IsNullOrEmpty(wtpContainer.WtpZipFile) && !string.IsNullOrEmpty(installDir) && File.Exists(wtpContainer.WtpZipFile);
            };
        }

        private const string GH_URI = "https://github.com/We-the-People-civ4col-mod/Mod/releases";


       
        private void AddLog(string message)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => AddLog(message))); return; }
            ActionLog.AppendText($"{DateTime.Now.ToLongTimeString()}\t{message}{Environment.NewLine}");
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {

            VersionList.Items.Clear();
            VersionList.Items.AddRange(wtpContainer.GetVerList().ToArray());
            VersionList.SelectedIndex = 0;
        }

        private void VersionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveButton.Enabled = VersionList.SelectedIndex >= 0;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (wtpContainer.DownloadInProgress) {
                wtpContainer.CancelDownload();
                SaveButton.Text = "Save";
                return;
            }

            SaveButton.Text = "Cancel";
            
            wtpContainer.DownloadProgressChanged += (s, ev) =>
            {
                progressBar.Value = ev.ProgressPercentage;
            };
            wtpContainer.DownloadFileCompleted += (s, ev) =>
            {
                progressBar.Visible = false;             
                SaveButton.Text = "Save";
            };
            wtpContainer.Download((string)VersionList.SelectedItem);




        }

        private string installDir = "";
        private void FindCivButton_Click(object sender, EventArgs e)
        {
            installDir = wtpContainer.FindCiv4ColDir();
          
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
                    var mgp = wtpContainer.FindCiv4ColMyGamesDir();
                        if (!string.IsNullOrEmpty(mgp)&&MessageBox.Show($"Do you want to use {mgp}?", "Success", MessageBoxButtons.YesNo) == DialogResult.Yes) installDir = mgp;
                    
                }
            }
           if (!string.IsNullOrEmpty(installDir)&&Directory.Exists(installDir))
            {
                installDir = Path.Combine(installDir, "Mods");
                AddLog($"Installation dir: {installDir}");
              
            } 
              
        }

        private void UnzipButton_Click(object sender, EventArgs e)
        {
          if (  wtpContainer.Unzip(installDir)&&MessageBox.Show("Remove downloaded file?","Success",MessageBoxButtons.YesNo)==DialogResult.Yes) wtpContainer.RemoveFile();
        }
    }
}
