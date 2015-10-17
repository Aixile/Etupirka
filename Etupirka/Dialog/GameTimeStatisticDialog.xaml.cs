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
	/// Interaction logic for GameTimeStatisticDialog.xaml
	/// </summary>
	public partial class GameTimeStatisticDialog : MetroWindow
	{
		List<TimeSummary> tlist;
		public GameTimeStatisticDialog(List<TimeSummary> t)
		{
			tlist = t;
			InitializeComponent();
			PlayTimeListView.ItemsSource = tlist;
		}
	}
}
