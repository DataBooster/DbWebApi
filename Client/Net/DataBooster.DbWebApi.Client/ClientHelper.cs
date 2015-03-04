// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
			HttpClient client = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });

			var header = client.DefaultRequestHeaders;
			header.Accept.Clear();
			header.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			return client;
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

					if (httpResponse.IsSuccessStatusCode)
					{
						var contentType = httpResponse.Headers.GetValues("Content-Type");
						return httpResponse.Content.ReadAsAsync<DbWebApiResponse>().Result;
					}
					else
					{
						var errorDictionary = httpResponse.Content.ReadAsAsync<HttpErrorClient>().Result;

						if (errorDictionary.Count == 0)
							throw new HttpRequestException(string.Format("{0} ({1})", (int)httpResponse.StatusCode, httpResponse.ReasonPhrase));
						else
							throw new HttpResponseClientException(errorDictionary);
					}
				}, cancellationToken);
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
	}
}
