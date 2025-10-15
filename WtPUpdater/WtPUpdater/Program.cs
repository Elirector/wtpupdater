using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WtPUpdater
{
	static class Program
	{
		/// <summary>
		/// Главная точка входа для приложения.
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
