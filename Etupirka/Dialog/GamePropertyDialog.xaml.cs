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
using System.ComponentModel;
using Microsoft.Win32;

namespace Etupirka.Dialog
{
	/// <summary>
	/// Interaction logic for GamePropertyDialog.xaml
	/// </summary>
	public partial class GamePropertyDialog : MetroWindow
	{
		GameExecutionInfo game,bgame;
		public GamePropertyDialog(GameExecutionInfo _game)
		{
			game = _game;
			bgame =(GameExecutionInfo) _game.Clone();
			updating = false;
			InitializeComponent();
			this.DataContext = bgame;
		}

		private bool updating;
		private void SyncESID_Click(object sender, RoutedEventArgs e)
		{
			if (updating)
			{
				return;
			}
			updating = true;
			bgame.updateInfoFromES();
			updating = false;
		}

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			game.Set(bgame);
			this.DialogResult = true;
		}

		private void GetProcPath_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Multiselect = false;
			openFileDialog1.Filter = "実行ファイル(*.exe)|*.exe|すべてのファイル(*.*)|*.*";
			if (openFileDialog1.ShowDialog() == true)
			{
				bgame.ProcPath = openFileDialog1.FileName;
			}
		}

		private void GetExecPath_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Multiselect = false;
			openFileDialog1.Filter = "実行ファイル(*.exe)|*.exe|すべてのファイル(*.*)|*.*";
			openFileDialog1.FilterIndex = 1;
			if (openFileDialog1.ShowDialog() == true)
			{
				bgame.ExecPath = openFileDialog1.FileName;
			}
		}

		private void GetProcPath_List_Click(object sender, RoutedEventArgs e)
		{	
			Dialog.ProcessDialog pd = new Dialog.ProcessDialog();
			pd.Owner = this;
			if (pd.ShowDialog() == true)
			{
				bgame.ProcPath=pd.SelectedProc.ProcPath;
			}
		}
	}
}
