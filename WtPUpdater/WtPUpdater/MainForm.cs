using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WtPUpdater
{
	public partial class MainForm : Form
	{

		private readonly WtpContainer _wtpContainer;
		private string _installDir = "";

		public MainForm()
		{
			InitializeComponent();

			_wtpContainer = new WtpContainer(GH_URI, AddLog);
			_wtpContainer.DownloadFileCompleted += (o, arg) =>
			{
				UnzipButton.Enabled = !string.IsNullOrEmpty(_wtpContainer.WtpZipFile) && !string.IsNullOrEmpty(_installDir) && File.Exists(_wtpContainer.WtpZipFile);
			};
		}

		//private const string GH_URI = "https://github.com/We-the-People-civ4col-mod/Mod/releases";
		private const string GH_URI = "https://api.github.com/repos/We-the-People-civ4col-mod/Mod/releases";



		private void AddLog(string message)
		{
			if (InvokeRequired) { BeginInvoke(new Action(() => AddLog(message))); return; }
			ActionLog.AppendText($"{DateTime.Now.ToLongTimeString()}\t{message}{Environment.NewLine}");
		}

		private void RefreshButton_Click(object sender, EventArgs e)
		{

			VersionList.Items.Clear();
			VersionList.Items.AddRange(_wtpContainer.GetVerList().Cast<object>().ToArray());
			if (VersionList.Items.Count > 0) VersionList.SelectedIndex = 0;
		}

		private void VersionList_SelectedIndexChanged(object sender, EventArgs e)
		{
			SaveButton.Enabled = VersionList.SelectedIndex >= 0;
		}

		private void SaveButton_Click(object sender, EventArgs e)
		{
			if (_wtpContainer.DownloadInProgress)
			{
				_wtpContainer.CancelDownload();
				SaveButton.Text = "Save";
				return;
			}

			SaveButton.Text = "Cancel";

			_wtpContainer.DownloadProgressChanged += (s, ev) =>
			{
				progressBar.Value = ev.ProgressPercentage;
			};
			_wtpContainer.DownloadFileCompleted += (s, ev) =>
			{
				progressBar.Visible = false;
				SaveButton.Text = "Save";
			};
			_wtpContainer.Download((string)VersionList.SelectedItem);




		}


		private void FindCivButton_Click(object sender, EventArgs e)
		{
			_installDir = _wtpContainer.FindCiv4ColDir();

			if (string.IsNullOrEmpty(_installDir))
			{
				if (MessageBox.Show("Civilization IV: Colonization install dir not found, do you like to select it manually?", "Find directory", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					var fod = new OpenFileDialog() { Filter = "Colonization.exe|Colonization.exe" };
					if (fod.ShowDialog() == DialogResult.OK && File.Exists(fod.FileName))
					{
						_installDir = Path.GetDirectoryName(fod.FileName);
					}
				}
				else
				{
					var mgp = _wtpContainer.FindCiv4ColMyGamesDir();
					if (!string.IsNullOrEmpty(mgp) && MessageBox.Show($"Do you want to use {mgp}?", "Success", MessageBoxButtons.YesNo) == DialogResult.Yes) _installDir = mgp;

				}
			}

			if (string.IsNullOrEmpty(_installDir) || !Directory.Exists(_installDir)) return;
			_installDir = Path.Combine(_installDir, "Mods", ModDirName.Text);
			AddLog($"Installation dir: {_installDir}");

		}

		private void UnzipButton_Click(object sender, EventArgs e)
		{
			if (_wtpContainer.Unzip(_installDir) && MessageBox.Show("Remove downloaded file?", "Success", MessageBoxButtons.YesNo) == DialogResult.Yes) _wtpContainer.RemoveFile();
			var postInstall = Path.Combine(_installDir, "setup.bat");
			if (File.Exists(postInstall))
			{
				AddLog("Post install setup file found, executing...");
				var psi = new ProcessStartInfo(postInstall);
				psi.WorkingDirectory = _installDir;
				psi.CreateNoWindow = true;
				Process.Start(psi);
			}
		}
	}
}
