// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DataBooster.DbWebApi.Client
{
	public static class ClientHelper
	{
		#region CreateClient extension methods
		public static HttpClient CreateClient(bool useDefaultCredentials = true)
		{
			if (useDefaultCredentials)
				return new HttpClient(new HttpClientHandler() { UseDefaultCredentials = useDefaultCredentials });
			else
				return new HttpClient();
		}

		public static HttpClient CreateClient(string baseAddress, bool useDefaultCredentials = true)
		{
			HttpClient client = CreateClient(useDefaultCredentials);

			if (!string.IsNullOrWhiteSpace(baseAddress))
				client.BaseAddress = new Uri(baseAddress);

			return client;
		}
		#endregion

		public static Task<DbWebApiResponse> ExecDbAsJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.ExecDbRawAsync(requestUri, inputParameters).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public static Task<DbWebApiResponse> ExecDbAsJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.ExecDbRawAsync(requestUri, inputParameters, cancellationToken).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public static DbWebApiResponse ExecDbAsJson(this HttpClient client, string requestUri, InputParameterDictionary inputParameters = null)
		{
			try
			{
				return client.ExecDbAsJsonAsync(requestUri, inputParameters).Result;
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

		public static DbWebApiResponse ExecDbAsJson(this HttpClient client, string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return client.ExecDbAsJson(requestUri, new InputParameterDictionary(anonymousTypeInstanceAsInputParameters));
		}

		public static Task<HttpResponseMessage> ExecDbRawAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken);
		}

		public static Task<HttpResponseMessage> ExecDbRawAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary());
		}

		public static DbWebApiResponse ReadDbJson(this HttpResponseMessage httpResponse)
		{
			var content = httpResponse.Content;
			var contentType = content.GetContentType();

			if (contentType != null && contentType.ToLower().EndsWith("/json") == false)
				throw new HttpRequestException("Response Content-Type is not JSON");

			if (httpResponse.IsSuccessStatusCode)
			{
				Task<DbWebApiResponse> readTask = content.ReadAsAsync<DbWebApiResponse>();
				DbWebApiResponse dbWebApiResponse = readTask.Result;

				if (readTask.IsFaulted)
					throw readTask.Exception;

				return dbWebApiResponse;
			}
			else
			{
				var errorDictionary = content.ReadAsAsync<HttpErrorClient>().Result;

				if (errorDictionary.Count == 0)
					throw new HttpRequestException(string.Format("{0} ({1})", (int)httpResponse.StatusCode, httpResponse.ReasonPhrase));
				else
					throw new HttpResponseClientException(errorDictionary);
			}
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
	}
}
