using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace Etupirka.Dialog
{
	/// <summary>
	/// Interaction logic for GameInfoDialog.xaml
	/// </summary>

	public partial class GameInfoDialog : MetroWindow
	{
		private ObservableCollection<GameInfo> items;
		public GameInfoDialog(List<GameInfo> list)
		{
			InitializeComponent();
			if (Properties.Settings.Default.disableGlowBrush)
			{
				this.GlowBrush = null;
			}

			items = new ObservableCollection<GameInfo>(list);
			GameInfoListView.ItemsSource = items;
		}
		

		public GameInfo SelectedGameInfo { get; set; }

		private void GameInfoListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var listView = (ListView)sender;
			var item = listView.ContainerFromElement((DependencyObject)e.OriginalSource) as ListViewItem;
			if (item != null)
			{
				int index = this.GameInfoListView.SelectedIndex;
				SelectedGameInfo = (GameInfo)this.GameInfoListView.SelectedItem;
				this.DialogResult = true;
			}
		}

		private void OpenES_Click(object sender, RoutedEventArgs e)
		{
			GameExecutionInfo g = (GameExecutionInfo)GameInfoListView.SelectedItem;
			if (g != null && g.ErogameScapeID != 0)
			{
				System.Diagnostics.Process.Start("http://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=" + g.ErogameScapeID);
			}
		}

		private void MetroWindow_Deactivated(object sender, EventArgs e)
		{
			GameInfoListView.UnselectAll();
		}
	}
}
