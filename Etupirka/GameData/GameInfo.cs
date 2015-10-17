using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Etupirka
{
	[Serializable]
	public class GameInfo : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected string uid;
		public string UID
		{
			get
			{
				return uid;
			}
			set
			{
				uid = value;
			}
		}

		public void GenerateUID()
		{
			var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			uid= new string(
				Enumerable.Repeat(chars, 16).Select(s => s[Utility.randomGen.Next(s.Length)]).ToArray());
			Console.WriteLine(uid);
		}

		public GameInfo() { erogameScapeID = -1; }
		public GameInfo(string _title)
		{
			title = _title;
			GenerateUID();
		}

		public GameInfo(int _erogameScapeID)
		{
			erogameScapeID = _erogameScapeID;
			GenerateUID();
			updateInfoFromES();
	
		}

		protected int erogameScapeID;
		public int ErogameScapeID
		{
			get { return erogameScapeID; }
			set { erogameScapeID = value; }
		}

		protected string title;
		public string Title
		{
			get { return title; }
			set { 
				title = value;
				OnPropertyChanged("Title");
			}
		}

		protected string brand;
		public string Brand
		{
			get { return brand; }
			set { 
				brand = value; 
				OnPropertyChanged("Brand");
			}
		}

		protected DateTime saleDay;
		public DateTime SaleDay
		{
			get { return saleDay; }
			set { 
				saleDay = value; 
				OnPropertyChanged("SaleDay");
				OnPropertyChanged("SaleDayString");
			}
		}
		public string SaleDayString
		{
			get {
				if (saleDay.Ticks != 0) return saleDay.ToShortDateString();
				else return "";
			}
		}
		public void updateInfoFromES()
		{
			if (Properties.Settings.Default.useOfflineESDatabase)
			{
				updateInfoFromESOffline();
			}
			else
			{
				updateInfoFromESOnline();
			}

		}

		public void updateInfoFromESOffline()
		{
			if (erogameScapeID > 0)
			{
				int pos = Utility.esInfo.getTermID(ErogameScapeID);
				if (pos >= 0)
				{
					Title = System.Net.WebUtility.HtmlDecode(Utility.esInfo.name[pos]);
					Brand = System.Net.WebUtility.HtmlDecode(Utility.esInfo.brand[pos]);
					SaleDay = DateTime.ParseExact(Utility.esInfo.saleday[pos], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
				}
			}
		} 

		public void updateInfoFromESOnline()
		{
			if (erogameScapeID > 0)
			{
				string result = NetworkUtility.GetString("http://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=" + erogameScapeID);
				//Console.Write(result);
				string matchTitle = "<div id=\"soft-title\"><span class=\"bold\">(?<title>.*?)</span>";
				string matchBrand = "<td><a href=\"brand.php\\?brand=(?<brandid>.*?)\">(?<brand>.*?)</a></td>";
				string matchSaleDay = "<td><a href=\"toukei_hatubaibi_month.php\\?.*\">(?<saleday>.*?)</a></td>";

				Regex re = new Regex(matchTitle, RegexOptions.IgnoreCase);
				for (Match m = re.Match(result); m.Success; m = m.NextMatch())
				{
					Title = System.Net.WebUtility.HtmlDecode(m.Groups["title"].Value);
					Console.Write(title);
				}
				re = new Regex(matchBrand, RegexOptions.IgnoreCase);
				for (Match m = re.Match(result); m.Success; m = m.NextMatch())
				{
					Brand = System.Net.WebUtility.HtmlDecode(m.Groups["brand"].Value);
					Console.Write(brand);
				}
				re = new Regex(matchSaleDay, RegexOptions.IgnoreCase);
				for (Match m = re.Match(result); m.Success; m = m.NextMatch())
				{
					string saleday = m.Groups["saleday"].Value;
					SaleDay = DateTime.ParseExact(saleday, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
					Console.Write(saleday);
				}
			}
		}
		
		protected void OnPropertyChanged(string name)
		{
				if (PropertyChanged == null) return;
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
