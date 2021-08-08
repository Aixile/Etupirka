using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace Etupirka.Dialogs
{
	/// <summary>
	/// Interaction logic for GlobalSettingDialog.xaml
	/// </summary>
	public partial class GlobalSettingDialog : MetroWindow
	{

		public GlobalSettingDialog()
		{
			InitializeComponent();
			if (Properties.Settings.Default.disableGlowBrush)
			{
				this.GlowBrush = null;
			}
		}

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default["proxyType"] = (int)NetworkView.MyProxyType;
			Properties.Settings.Default["proxyAddress"] = NetworkView.ProxyAddress;
			Properties.Settings.Default["proxyPort"] = NetworkView.ProxyPort;
			Properties.Settings.Default["proxyUser"] = NetworkView.ProxyUser;
			Properties.Settings.Default["proxyPassword"] = NetworkView.passwordBox.Password;
			Properties.Settings.Default.differExecuatableGame = GeneralView.DifferExecuatableGame;
			Properties.Settings.Default.setStartUp = GeneralView.SetStartUp;
			Properties.Settings.Default.watchProcess = GeneralView.WatchProcess;
			Properties.Settings.Default.monitorInterval = GeneralView.MontiorInterval;
			Properties.Settings.Default.playVoice = GeneralView.PlayVoice;
			Properties.Settings.Default.minimizeAtStartup = GeneralView.MinimizeAtStartup;
			Properties.Settings.Default.askBeforeExit = GeneralView.AskBeforeExit;
			Properties.Settings.Default.checkUpdate = GeneralView.CheckUpdate;
			Properties.Settings.Default.disableGlowBrush = GeneralView.DisableGlowBrush;
            Properties.Settings.Default.hideListWhenPlaying = GeneralView.HideListWhenPlaying;
            Properties.Settings.Default.useOfflineESDatabase = DatabaseView.UseOfflineDatabase;
			Properties.Settings.Default.Save();
			this.DialogResult = true;
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.Dispatcher.BeginInvoke(new Action(() =>
			{
				this.ConfigTab.Focus();
			}));
		}
	}
}
