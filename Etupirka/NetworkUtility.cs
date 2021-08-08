using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections;
using Etupirka.Views;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace Etupirka
{
	public static class NetworkUtility
	{
		private class MyWebClient : WebClient
		{
			public int timeoutMs { get; set; } = 10000;

			public MyWebClient()
            {
				configureWebClient(this);
			}

			protected override WebRequest GetWebRequest(Uri uri)
			{
				WebRequest w = base.GetWebRequest(uri);
				w.Timeout = timeoutMs;
				return w;
			}

			private static void setProxy(WebClient wc)
			{
				int pt = Properties.Settings.Default.proxyType;
				if (pt == 1)
				{
					WebProxy proxy = new WebProxy(Properties.Settings.Default.proxyAddress, Properties.Settings.Default.proxyPort);
					if (!string.IsNullOrEmpty(Properties.Settings.Default.proxyUser))
					{
						proxy.Credentials = new NetworkCredential(Properties.Settings.Default.proxyUser, Properties.Settings.Default.proxyPassword);
					}
					wc.Proxy = proxy;
				}
			}

			private static void configureWebClient(WebClient wc)
			{
				wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36");
				wc.Encoding = System.Text.Encoding.UTF8;
				setProxy(wc);
			}
		}

		public static string PostString(string uri, string data)
        {
			using (var client = new MyWebClient())
			{
				return client.UploadString(uri, data);
			}
        }

		public static Task<string> GetString(string uri)
		{
			using (var client = new MyWebClient())
			{
				return client.DownloadStringTaskAsync(uri);
			}
		}

		public static Task<byte[]> GetData(string uri)
		{
			using (var client = new MyWebClient())
			{
				return client.DownloadDataTaskAsync(uri);
			}
		}

		public static async Task<HtmlAgilityPack.HtmlDocument> GetHtmlDocument(string uri)
		{
			using (var client = new MyWebClient())
			{
				string str = await client.DownloadStringTaskAsync(new Uri(uri));
				var document = new HtmlAgilityPack.HtmlDocument();
				document.LoadHtml(str);
				return document;
			}
		}
	}
}
