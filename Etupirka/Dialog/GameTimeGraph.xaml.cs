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
using De.TorstenMandelkow.MetroChart;
using MahApps.Metro.Controls;

namespace Etupirka.Dialog
{
	/// <summary>
	/// Interaction logic for GameTimeGraph.xaml
	/// </summary>
	public partial class GameTimeGraph : MetroWindow
	{
		List<GameTimeSummary> glist;
		public GameTimeGraph(List<GameTimeSummary> g,string subtitle)
		{
			glist = g;
			InitializeComponent();
			crt.ItemsSource = glist;
			chart.SelectedBrush = null;
			chart.ChartSubTitle = subtitle;
		}

	}
}
