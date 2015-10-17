using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SatoruErogeTimer
{
	[Serializable]
	public class Eroge
	{
		public Eroge() { }
		public Eroge(string _title, int _time)
		{
			title = _title;
			time = _time;
		}
		protected string title;
		public string Title
		{
			get { return title; }
			set { title = value; }
		}
		protected int time;
		public int Time
		{
			get { return time; }
			set { time = value; }
		}
		public void addTime(int i)
		{
			time += i;
		}
		public string getTime()
		{
			int m = (time / 60);
			int h = m / 60;
			m %= 60;
			return h + @"時間" + m + @"分";
		}
	}
}
