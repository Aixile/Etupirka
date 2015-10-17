using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections;
using Etupirka.Views;

namespace Etupirka
{
	public static class NetworkUtility
	{
		public static string PostString(string uri,string data)
		{
			WebClient wc = new WebClient();
			string result= wc.UploadString(uri, data);
			return result;
		}

		private static WebClient setProxy(WebClient wc)
		{
			int pt = (int)Properties.Settings.Default["proxyType"];
			if (pt == 0) return wc;
			if (pt == 1)
			{
				WebProxy proxy = new WebProxy((string)Properties.Settings.Default["proxyAddress"],(int)Properties.Settings.Default["proxyPort"]);
				wc.Proxy = proxy;
			}
			return wc;
		}
		public static string Get(string uri)
		{
			WebClient wc = new WebClient();
			wc = setProxy(wc);

			wc.Encoding = System.Text.Encoding.UTF8;
			try
			{
				string result = wc.DownloadString(uri);
				return result;

			}
			catch
			{
				return "";
			}
		}
	}
}
