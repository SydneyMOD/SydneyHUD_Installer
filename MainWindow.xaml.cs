using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace SydneyHUD_Installer
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			MouseLeftButtonDown += (sender, e) => DragMove();
			Log(Properties.Resources.Loaded);
		}

		public static string PD2PATH;
		public static bool WORKABLE = true;
		public static WebClient client = null;
		public static string PDMOD_API = "http://download.paydaymods.com/download/latest/";
		public static string mod_name;
		public static string[] INSTALL_MODS = new string[] { "payday2blt", "payday2bltdll", "SydneyHUD", "SydneyHUDAssets" };

		public void Log(string text, bool status_bar = false)
		{
			TextBox_log.Text += "\n" + text;
			if (status_bar == true)
			{
				Label_status.Content = text;
			}
		}

		public void ChangeThemeColor(string color)
		{
			object obj = ColorConverter.ConvertFromString(color);
			SolidColorBrush brush = new SolidColorBrush((Color)obj);
			Border.BorderBrush = brush;
			Footer.Background = brush;
		}

		private void Button_exit_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Button_directory_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				Title = Properties.Resources.Selector_title,
				Filter = "payday2_win32_release.exe|*.exe",
				InitialDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\PAYDAY 2"
			};
			if (dialog.ShowDialog() == true)
			{
				TextBox_path.Text = dialog.FileName;
			}
		}

		private void Button_install_Click(object sender, RoutedEventArgs e)
		{
			PD2PATH = System.IO.Path.GetDirectoryName(TextBox_path.Text);

			while (WORKABLE)
			{
				foreach(string mod in INSTALL_MODS)
				{
					Download(mod);
				}
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{

		}

		public void Download(string URL)
		{
			Uri url = new Uri(PDMOD_API + URL);
			mod_name = url.Segments.Last();
			string path = PD2PATH + $@"\mods\downloads\{mod_name}.zip";
			
			if (client == null)
			{
				client = new WebClient();
				client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Client_DownloadProgressChanged);
				client.DownloadFileCompleted += new AsyncCompletedEventHandler(Client_DownloadFileCompleted);
			}
			Log(Properties.Resources.Start_download + mod_name, true);
			client.DownloadFileTaskAsync(url, path);
		}
		private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			ProgressBar.Value = e.ProgressPercentage;
			Label_status.Content = $"{e.BytesReceived} / {e.TotalBytesToReceive} bytes...";
		}
		private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				ChangeThemeColor("FF9800");
				Log(Properties.Resources.Cancel_download, true);
				WORKABLE = false;
			}
			else if (e.Error != null)
			{
				ChangeThemeColor("f44336");
				Log(Properties.Resources.Error, true);
				Log(e.Error.Message);
				WORKABLE = false;
			}
			else
			{
				Log(Properties.Resources.Done_download);
				Extract();
			}
		}

		public void Extract()
		{
			string src = PD2PATH + $@"\mods\downloads\{mod_name}.zip";
			string dest;

			switch(mod_name)
			{
				case "payday2bltdll":
					dest = PD2PATH;
					break;

				case "SydneyHUDAssets":
					dest = PD2PATH + @"\assets\mod_overrides\";
					break;

				default:
					dest = PD2PATH = @"\mods\";
					break;
			}

			if (!Directory.Exists(dest))
			{
				Directory.CreateDirectory(dest);
			}

			try
			{
				Log(Properties.Resources.Extracting + dest);
				ZipFile.ExtractToDirectory(src, dest, Encoding.UTF8);
			}
			catch (Exception e)
			{
				ChangeThemeColor("f44336");
				Log(Properties.Resources.Error , true);
				Log(e.Message);
				WORKABLE = false;
			}
		}
	}
}
