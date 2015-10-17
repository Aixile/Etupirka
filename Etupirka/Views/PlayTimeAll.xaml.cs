using Etupirka.Dialog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Etupirka.Views
{
	/// <summary>
	/// Interaction logic for PlayTimeAll.xaml
	/// </summary>
	public partial class PlayTimeAll : UserControl
	{
	private SQLiteConnection conn;
		private List<TimeSummary> sd;

		public PlayTimeAll()
		{
			sd = new List<TimeSummary>();
			InitializeComponent();
			PlayTimeListView.ItemsSource = sd;
		}

		public void Setup(SQLiteConnection _conn){
			conn = _conn;
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				command.CommandText = @"SELECT SUM(p.playtime) t ,p.datetime  
										FROM games g,playtime p 
										WHERE g.uid=p.game  
										GROUP BY p.datetime
										ORDER BY p.datetime DESC";
				using (SQLiteDataReader reader = command.ExecuteReader())
				{
						while (reader.Read())
						{
							sd.Add(new TimeSummary(reader["datetime"].ToString(),
								Convert.ToInt32(reader["t"].ToString())));

						}
				}
				conn.Close();
			}
		}

		private void PlayTimeListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var listView = (ListView)sender;
			var item = listView.ContainerFromElement((DependencyObject)e.OriginalSource) as ListViewItem;
			if (item != null)
			{
				TimeSummary g = (TimeSummary)PlayTimeListView.SelectedItem;
				if (g.time != 0)
				{
					List<GameTimeSummary> glist = new List<GameTimeSummary>();

					using (SQLiteCommand command = conn.CreateCommand())
					{
						conn.Open();
						command.CommandText = @"SELECT  g.title, p.playtime FROM games g,playtime p 
											WHERE g.uid=p.game AND p.datetime=date('"
											+ g.d.ToString("yyyy-MM-dd") + @"') ";
						using (SQLiteDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								glist.Add(new GameTimeSummary(reader["title"].ToString(), Convert.ToInt32(reader["playtime"].ToString())));
							}
						}
					}
					conn.Close();
					Dialog.GameTimeGraph gtg = new GameTimeGraph(glist, g.d.ToString("yyyy-MM-dd"));
					gtg.Owner = Window.GetWindow(this);
					if (gtg.ShowDialog() == true)
					{

					}
				}
			}
		}
	}
}
