// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DataBooster.DbWebApi.Client
{
	public static class ClientHelper
	{
		public static HttpClient CreateClient()
		{
			return new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });
		}

		public static Task<DbWebApiResponse> RequestJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.RequestJsonAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public static Task<DbWebApiResponse> RequestJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					HttpResponseMessage httpResponse = requestTask.Result;
					var content = httpResponse.Content;
					var contentType = content.GetContentType();

					if (contentType != null && contentType.ToLower().EndsWith("/json") == false)
						throw new HttpRequestException("Response Content-Type is not JSON");

					if (httpResponse.IsSuccessStatusCode)
						return content.ReadAsAsync<DbWebApiResponse>().Result;
					else
					{
						var errorDictionary = content.ReadAsAsync<HttpErrorClient>().Result;

						if (errorDictionary.Count == 0)
							throw new HttpRequestException(string.Format("{0} ({1})", (int)httpResponse.StatusCode, httpResponse.ReasonPhrase));
						else
							throw new HttpResponseClientException(errorDictionary);
					}
				}, cancellationToken);
		}

		private static string GetContentType(this HttpContent content)
		{
			if (content == null)
				return null;
			if (content.Headers == null)
				return null;
			if (content.Headers.ContentType == null)
				return null;
			return content.Headers.ContentType.MediaType;
		}

		public static DbWebApiResponse RequestJson(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			try
			{
				return client.RequestJsonAsync(requestUri, inputParameters).Result;
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

		public static Task<Tuple<HttpContentHeaders, Stream>> RequestStreamAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.RequestStreamAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public static Task<Tuple<HttpContentHeaders, Stream>> RequestStreamAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken).
				ContinueWith<Tuple<HttpContentHeaders, Stream>>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					HttpResponseMessage httpResponse = requestTask.Result;
					HttpContent content = httpResponse.EnsureSuccessStatusCode().Content;

					return Tuple.Create(content.Headers, content.ReadAsStreamAsync().Result);
				}, cancellationToken);
		}

		public static Task<Tuple<HttpContentHeaders, string>> RequestStringAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.RequestStringAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public static Task<Tuple<HttpContentHeaders, string>> RequestStringAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken).
				ContinueWith<Tuple<HttpContentHeaders, String>>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					HttpResponseMessage httpResponse = requestTask.Result;
					HttpContent content = httpResponse.EnsureSuccessStatusCode().Content;

					return Tuple.Create(content.Headers, content.ReadAsStringAsync().Result);
				}, cancellationToken);
		}
	}
}
