using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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

		public async Task updateInfoFromES()
		{
			if (Properties.Settings.Default.useOfflineESDatabase)
			{
				updateInfoFromESOffline();
			}
			else
			{
				await updateInfoFromESOnline();
			}
		}

		public void updateInfoFromESOffline()
		{
			if (erogameScapeID > 0)
			{
				Utility.im.getEsInfo(this);
			}
		} 

		public async Task updateInfoFromESOnline()
		{
			if (erogameScapeID > 0)
			{
                var uri = "http://erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=" + erogameScapeID;
                if (Properties.Settings.Default.useGoogleCache)
                {
                    uri = "https://webcache.googleusercontent.com/search?q=cache:" + HttpUtility.UrlEncode(uri) + "&strip=1";
                }
                var document = await NetworkUtility.GetHtmlDocument(uri);
				Title = HttpUtility.HtmlDecode(document.GetElementbyId("game_title").InnerText);
				Brand = HttpUtility.HtmlDecode(document.GetElementbyId("brand").Descendants("td").First().InnerText);
				string saleday = HttpUtility.HtmlDecode(document.GetElementbyId("sellday").Descendants("td").First().InnerText);
				SaleDay = DateTime.ParseExact(saleday, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
			}
		}
		
		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged == null) return;
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
