using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Etupirka
{
	public class TimeData 
	{
		public Dictionary<string, int> d;
		public TimeData()
		{
			d = new Dictionary<string, int>();
		}

		public TimeData(Dictionary<string,int> _d){
			d = _d;
		}

		public void AddTime(string uid, int value)
		{
			if (!d.ContainsKey(uid))
			{
				d.Add(uid, value);
			}
			else
			{
				d[uid] += value;

			}
		}

		public int GetSum()
		{
			int tot = 0;
			foreach (KeyValuePair<string, int> i in d)
			{
				tot += i.Value;
			}
			return tot;
		}

		public int GetValue(string uid)
		{
			if (!d.ContainsKey(uid))
			{
				return 0;
			}
			return d[uid];
		}
	}
}
