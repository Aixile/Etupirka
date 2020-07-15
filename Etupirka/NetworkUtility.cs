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
using Timer = System.Threading.Timer;

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
			int pt = Properties.Settings.Default.proxyType;
			if (pt == 0) return wc;
			if (pt == 1)
			{
				WebProxy proxy = new WebProxy(Properties.Settings.Default.proxyAddress,Properties.Settings.Default.proxyPort);
				if (!String.IsNullOrEmpty(Properties.Settings.Default.proxyUser))
				{
					proxy.Credentials = new NetworkCredential(Properties.Settings.Default.proxyUser, Properties.Settings.Default.proxyPassword);
				}
				wc.Proxy = proxy;
			}
			return wc;
		}

		public static string GetString(string uri)
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

		public static byte[] GetData(string uri)
		{
			WebClient wc = new WebClient();
			wc = setProxy(wc);

			wc.Encoding = System.Text.Encoding.UTF8;
			try
			{
				byte[] result = wc.DownloadData(uri);
				return result;

			}
			catch
			{
				return null;
			}
		}

		public static Task<HtmlDocument> GetHtmlDocument(string uri, int timeoutMs = 10000)
		{
			var browser = new WebBrowser();
			var taskCompletionSource = new TaskCompletionSource<HtmlDocument>();
			Timer timer = new Timer((state) => taskCompletionSource.TrySetException(new TimeoutException()), null, timeoutMs, Timeout.Infinite);
			browser.DocumentCompleted += (sender, e) => taskCompletionSource.TrySetResult(browser.Document);
			browser.ScriptErrorsSuppressed = true;
			browser.Navigate(uri);
			return taskCompletionSource.Task;
		}
	}
}
