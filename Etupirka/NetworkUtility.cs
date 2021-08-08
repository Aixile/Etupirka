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
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Etupirka
{
	public static class NetworkUtility
	{
		private class MyWebClient : IDisposable
		{
			public int TimeoutMs { get; set; } = 10000;

			private HttpClient client = new HttpClient();

			public MyWebClient()
            {
				HttpClientHandler clientHandler = new HttpClientHandler()
				{
					Proxy = GetProxyOrNull(),
				};
				client = new HttpClient(clientHandler);
				client.Timeout = TimeSpan.FromMilliseconds(TimeoutMs);
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36");
			}

			public void Dispose()
			{
				client.Dispose();
			}

			public async Task<string> Get(string uri, CancellationToken cancellationToken = new CancellationToken())
            {
				var response = await client.GetAsync(uri, cancellationToken);
				return await ProcessResponse(response);
			}

			public async Task<string> Post(string uri, HttpContent content, CancellationToken cancellationToken = new CancellationToken())
			{
				var response = await client.PostAsync(uri, content, cancellationToken);
				return await ProcessResponse(response);
			}

			private async Task<string> ProcessResponse(HttpResponseMessage response)
            {
				response.EnsureSuccessStatusCode();
				var bytes = await response.Content.ReadAsByteArrayAsync();
				return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			}

			private static WebProxy GetProxyOrNull()
			{
				int pt = Properties.Settings.Default.proxyType;
				if (pt == 1)
				{
					WebProxy proxy = new WebProxy(Properties.Settings.Default.proxyAddress, Properties.Settings.Default.proxyPort);
					if (!string.IsNullOrEmpty(Properties.Settings.Default.proxyUser))
					{
						proxy.Credentials = new NetworkCredential(Properties.Settings.Default.proxyUser, Properties.Settings.Default.proxyPassword);
					}
					return proxy;
				}
				return null;
			}
        }

		public static async Task<string> GetString(string uri, CancellationToken cancellationToken = new CancellationToken(), int timeoutMs = 10000)
		{
			using (var client = new MyWebClient() { TimeoutMs = timeoutMs })
			{
				return await client.Get(uri, cancellationToken);
			}
		}

		public static async Task<string> PostString(string uri, HttpContent content, CancellationToken cancellationToken = new CancellationToken(), int timeoutMs = 10000)
		{
			using (var client = new MyWebClient() { TimeoutMs = timeoutMs })
			{
				return await client.Post(uri, content, cancellationToken);
			}
		}

		public static async Task<JToken> GetJson(string uri, CancellationToken cancellationToken = new CancellationToken(), int timeoutMs = 10000)
		{
			return JToken.Parse(await GetString(uri, cancellationToken, timeoutMs));
		}

		public static async Task<HtmlAgilityPack.HtmlDocument> GetHtmlDocument(string uri, CancellationToken cancellationToken = new CancellationToken(), int timeoutMs = 10000)
		{
			var document = new HtmlAgilityPack.HtmlDocument();
			document.LoadHtml(await GetString(uri, cancellationToken, timeoutMs));
			return document;
		}

		public static async Task<HtmlAgilityPack.HtmlDocument> PostHtmlDocument(string uri, HttpContent content, CancellationToken cancellationToken = new CancellationToken(), int timeoutMs = 10000)
		{
			var document = new HtmlAgilityPack.HtmlDocument();
			document.LoadHtml(await PostString(uri, content, cancellationToken, timeoutMs));
			return document;
		}
	}
}
