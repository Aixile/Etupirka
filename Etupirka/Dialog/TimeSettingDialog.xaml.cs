using MahApps.Metro.Controls;
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

namespace Etupirka.Dialog
{
	/// <summary>
	/// Interaction logic for TimeSettingDialog.xaml
	/// </summary>
	public partial class TimeSettingDialog : MetroWindow
	{
		GameExecutionInfo game, bgame;
		public TimeSettingDialog(GameExecutionInfo g)
		{
			game = g;
			bgame =(GameExecutionInfo) g.Clone();
			this.DataContext = bgame;
			InitializeComponent();
			tc.DataContext = bgame;
		}

		private void btnDialogOk_Click(object sender, RoutedEventArgs e)
		{
			game.Set(bgame);
			this.DialogResult = true;
		}

	}
}
