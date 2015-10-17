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
	/// Interaction logic for PlayTimeWeek.xaml
	/// </summary>
	public partial class PlayTimeWeek : UserControl
	{
		private SQLiteConnection conn;
		private List<TimeSummary> sd;

		public PlayTimeWeek()
		{
			sd = new List<TimeSummary>();
			InitializeComponent();
			PlayTimeListView.ItemsSource = sd;
		}
		public void Setup(SQLiteConnection _conn)
		{
			conn = _conn;
			using (SQLiteCommand command = conn.CreateCommand())
			{
				conn.Open();
				DateTime t = new DateTime(2015, 8, 31);
				DateTime d = DateTime.Today;
				while (d.DayOfWeek != DayOfWeek.Monday) d=d.AddDays(-1);
				while(d>=t)
				{
					command.CommandText = @"SELECT SUM(p.playtime) t
										FROM games g,playtime p 
										WHERE g.uid=p.game AND date(p.datetime) between date('" + d.ToString("yyyy-MM-dd") + @"') AND
																						date('" + d.AddDays(6).ToString("yyyy-MM-dd") + @"')";
					using (SQLiteDataReader reader = command.ExecuteReader())
					{

						if (reader.Read())
						{
							if (reader.IsDBNull(0))
							{
								sd.Add(new TimeSummary(d.ToString("yyyy-MM-dd"), 0));

							}else{
								sd.Add(new TimeSummary(d.ToString("yyyy-MM-dd"),
									Convert.ToInt32(reader["t"].ToString())));
							}
						}
						else
						{
							sd.Add(new TimeSummary(d.ToString("yyyy-MM-dd"), 0));
						}
					}
					d=d.AddDays(-7);
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
						command.CommandText = @"SELECT g.title,SUM(p.playtime) playtime
											FROM  games g,playtime p 
											WHERE g.uid=p.game AND p.datetime BETWEEN 
											date('" + g.d.ToString("yyyy-MM-dd") + @"') AND 
											date('" + g.d.AddDays(6).ToString("yyyy-MM-dd") + @"')
											GROUP BY g.uid";
						using (SQLiteDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								glist.Add(new GameTimeSummary(reader["title"].ToString(), Convert.ToInt32(reader["playtime"].ToString())));
							}
						}
					}
					conn.Close();
					Dialog.GameTimeGraph gtg = new GameTimeGraph(glist, g.d.ToString("yyyy-MM-dd") +"~"+ g.d.AddDays(6).ToString("yyyy-MM-dd"));
					gtg.Owner = Window.GetWindow(this);
					if (gtg.ShowDialog() == true)
					{

					}

				}

			}
		}

	}
}
