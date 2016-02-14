using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
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
	/// Interaction logic for PlayTimeStatisticDialog.xaml
	/// </summary>
	public class TimeSummary
	{
		public TimeSummary(string dat, int val)
		{
			d = DateTime.ParseExact(dat, "yyyy-MM-dd", CultureInfo.InvariantCulture);
			time = val;
		}
		public DateTime d { get; set; }
		public string DayString
		{
			get{
				return d.ToString("yyyy-MM-dd");
			}
		}

		public string MonthString
		{
			get
			{
				return d.ToString("yyyy-MM");
			}
		}

		public int time { get; set; }
		public string TimeString
		{
			get{
				int s = time % 60;
				int m = (time / 60);
				int h = m / 60;
				m %= 60;
				return h + @"時間" + m + @"分" + s + @"秒";
			}
		}
	}

	public class GameTimeSummary
	{
		public GameTimeSummary(string game, int time)
		{
			Game = game;
			t = time;

		}
		int t;
		public string Game{ get;set; }
		public double PlayTime
		{
			get
			{
				return Math.Round((Convert.ToDouble(t) / 60), 2);
			}
		}

	}

	public partial class PlayTimeStatisticDialog : MetroWindow
	{

		public SQLiteConnection conn;

		public PlayTimeStatisticDialog(string db_path)
		{
			InitializeComponent();
			if (Properties.Settings.Default.disableGlowBrush)
			{
				this.GlowBrush = null;
			}

			conn = new SQLiteConnection("Data Source=" + db_path);
			if (File.Exists(db_path))
			{
				PlayTimeLast30Days.Setup(conn);
				PlayTimeWeek.Setup(conn);
				PlayTimeMonth.Setup(conn);
				PlayTimeAll.Setup(conn);
			}
		}
	}
}
