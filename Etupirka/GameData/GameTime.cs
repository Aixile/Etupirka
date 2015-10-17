using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etupirka
{
	[Serializable]
	public class GameTime: GameInfo
	{
		public GameTime()
		{

		}
		public GameTime(int ESID)
		{
			erogameScapeID = ESID;
			GenerateUID();
			try
			{
				updateInfoFromES();
			}
			catch
			{

			}
			totalPlayTime = 0;
			firstPlayTime = new DateTime(0);
			lastPlayTime = new DateTime(0);
		}

		protected int totalPlayTime;
		public int TotalPlayTime
		{
			get { return totalPlayTime; }
			set {
				totalPlayTime = value;
				OnPropertyChanged("TotalPlayTime");
				OnPropertyChanged("TotalPlayTimeString");
			}
		}
		public string TotalPlayTimeString
		{
			get
			{
				if (totalPlayTime > 0)
				{
					int m = (totalPlayTime / 60);
					int h = m / 60;
					m %= 60;
					return h + @"時間" + m + @"分";
				}
				else
				{
					return "";
				}
			}
		}

		protected DateTime firstPlayTime;
		public DateTime FirstPlayTime
		{
			get { return firstPlayTime; }
			set
			{
				firstPlayTime = value;
				OnPropertyChanged("FirstPlayTime");
				OnPropertyChanged("FirstPlayTimeString");
			}
		}
		public string FirstPlayTimeString
		{
			get
			{
				if (firstPlayTime.Ticks!=0)
					return firstPlayTime.ToString("MM/dd/yyyy HH:mm");
				else return "";
			}
		}

		protected DateTime lastPlayTime;
		public DateTime LastPlayTime
		{
			get { return lastPlayTime; }
			set
			{
				lastPlayTime = value;
				OnPropertyChanged("LastPlayTime");
				OnPropertyChanged("LastPlayTimeString");
			}
		}
		public string LastPlayTimeString
		{
			get
			{
				if (lastPlayTime.Ticks!=0)
					return lastPlayTime.ToString("MM/dd/yyyy HH:mm");
				else return "";
			}
		}

		public void addTime(int t){
			TotalPlayTime = totalPlayTime + t;
		}
	}
}
