// Copyright (c) 2017 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DataBooster.DbWebApi.Client
{
	public partial class DbWebApiClient
	{
		private const string _exceptionGetWithBody = "HTTP-Get-Method cannot carry content body";

		#region ExecAsJson overloads
		public Task<JObject> ExecAsJsonAsync(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<JObject>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public JObject ExecAsJson(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<JObject>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public Task<JObject> ExecAsJsonAsync(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<JObject>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}

		public JObject ExecAsJson(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<JObject>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}
		#endregion

		#region ExecAsXml overloads
		public Task<XDocument> ExecAsXmlAsync(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<XDocument>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public XDocument ExecAsXml(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<XDocument>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<XDocument>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}

		public XDocument ExecAsXml(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<XDocument>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}
		#endregion

		#region ExecAsString overloads
		public Task<string> ExecAsStringAsync(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<string>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public string ExecAsString(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<string>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public Task<string> ExecAsStringAsync(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<string>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}

		public string ExecAsString(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<string>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}
		#endregion

		#region ExecAsStream overloads
		public Task<Stream> ExecAsStreamAsync(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<Stream>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public Stream ExecAsStream(string requestUri, string content, Encoding encoding = null, string mediaType = null, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<Stream>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		public Task<Stream> ExecAsStreamAsync(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAsAsync<Stream>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}

		public Stream ExecAsStream(string requestUri, Stream content, int bufferSize = 0, Action<HttpContentHeaders> contentHeadersCustomizer = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return ExecAs<Stream>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}
		#endregion

		#region Exec as Generic overloads
#if WEB_API2
		protected async Task<T> ExecAsAsync<T>(string requestUri, string content, Encoding encoding, string mediaType, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken) where T : class
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken).ConfigureAwait(false);
			return httpResponse.ReadAs<T>();
		}

		protected async Task<T> ExecAsAsync<T>(string requestUri, Stream content, int bufferSize, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken) where T : class
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken).ConfigureAwait(false);
			return httpResponse.ReadAs<T>();
		}
#else	// ASP.NET Web API 1
		protected Task<T> ExecAsAsync<T>(string requestUri, string content, Encoding encoding, string mediaType, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken) where T : class
		{
			return ExecRawAsync(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken).
				ContinueWith<T>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadAs<T>();
				});
		}

		protected Task<T> ExecAsAsync<T>(string requestUri, Stream content, int bufferSize, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken) where T : class
		{
			return ExecRawAsync(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken).
				ContinueWith<T>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadAs<T>();
				});
		}
#endif

		protected T ExecAs<T>(string requestUri, string content, Encoding encoding, string mediaType, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken) where T : class
		{
			try
			{
				return ExecAsAsync<T>(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken).Result;
			}
			catch (AggregateException ae)
			{
				if (ae.InnerExceptions.Count == 1)
				{
					Exception eInner = ae.InnerException;

					if (eInner.InnerException != null)
						eInner = eInner.InnerException;

					throw eInner;
				}
				else
					throw;
			}
		}

		protected T ExecAs<T>(string requestUri, Stream content, int bufferSize, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken) where T : class
		{
			try
			{
				return ExecAsAsync<T>(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken).Result;
			}
			catch (AggregateException ae)
			{
				if (ae.InnerExceptions.Count == 1)
				{
					Exception eInner = ae.InnerException;

					if (eInner.InnerException != null)
						eInner = eInner.InnerException;

					throw eInner;
				}
				else
					throw;
			}
		}
		#endregion

		#region Raw Methods
		protected Task<HttpResponseMessage> ExecRawAsync(string requestUri, string content, Encoding encoding, string mediaType, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken)
		{
			if (_HttpMethod == HttpMethod.Get)
			{
				if (string.IsNullOrEmpty(content))
					return _HttpClient.GetAsync(requestUri, cancellationToken);
				else
					throw new InvalidOperationException(_exceptionGetWithBody);
			}
			else
				return PostRawAsync(requestUri, content, encoding, mediaType, contentHeadersCustomizer, cancellationToken);
		}

		private Task<HttpResponseMessage> PostRawAsync(string requestUri, string content, Encoding encoding, string mediaType, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken)
		{
			StringContent stringContent = new StringContent(content, encoding, mediaType);

			if (contentHeadersCustomizer != null)
				contentHeadersCustomizer(stringContent.Headers);

			return _HttpClient.PostAsync(requestUri, stringContent, cancellationToken);
		}

		protected Task<HttpResponseMessage> ExecRawAsync(string requestUri, Stream content, int bufferSize, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken)
		{
			if (_HttpMethod == HttpMethod.Get)
			{
				if (content == null)
					return _HttpClient.GetAsync(requestUri, cancellationToken);
				else
					throw new InvalidOperationException(_exceptionGetWithBody);
			}
			else
				return PostRawAsync(requestUri, content, bufferSize, contentHeadersCustomizer, cancellationToken);
		}

		private Task<HttpResponseMessage> PostRawAsync(string requestUri, Stream content, int bufferSize, Action<HttpContentHeaders> contentHeadersCustomizer, CancellationToken cancellationToken)
		{
			StreamContent streamContent = (bufferSize > 0) ? new StreamContent(content, bufferSize) : new StreamContent(content);

			if (contentHeadersCustomizer != null)
				contentHeadersCustomizer(streamContent.Headers);

			return _HttpClient.PostAsync(requestUri, streamContent, cancellationToken);
		}
		#endregion
	}
}
