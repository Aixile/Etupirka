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
			
			InitializeComponent();
			this.DataContext = bgame;
			bgame.PropertyChanged += TitleChanged;
		}
		private static void TitleChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName!="Title") return;
			var p=(GameExecutionInfo) sender;
			// 文字列でプロパティ名を判別
			Console.WriteLine("名前が変更されました: " + p.Title);
		}
		private void SyncESID_Click(object sender, RoutedEventArgs e)
		{
			bgame.updateInfoFromES();
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
	}
}
