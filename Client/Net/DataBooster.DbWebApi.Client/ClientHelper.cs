// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataBooster.DbWebApi.Client
{
	public static class ClientHelper
	{
		const string _DefaultJsonInputParameterName = "JsonInput";
		private static string _JsonInputParameterName = _DefaultJsonInputParameterName;
		public static string JsonInputParameterName
		{
			get { return _JsonInputParameterName; }
			set { _JsonInputParameterName = string.IsNullOrEmpty(value) ? _DefaultJsonInputParameterName : value; }
		}

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

		#region Http Post
		public static Task<DbWebApiResponse> PostDbAsJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.PostDbRawAsync(requestUri, inputParameters).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public static Task<DbWebApiResponse> PostDbAsJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.PostDbRawAsync(requestUri, inputParameters, cancellationToken).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public static DbWebApiResponse PostDbAsJson(this HttpClient client, string requestUri, InputParameterDictionary inputParameters = null)
		{
			try
			{
				return client.PostDbAsJsonAsync(requestUri, inputParameters).Result;
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

		public static DbWebApiResponse PostDbAsJson(this HttpClient client, string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return client.PostDbAsJson(requestUri, new InputParameterDictionary(anonymousTypeInstanceAsInputParameters));
		}

		public static Task<HttpResponseMessage> PostDbRawAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken);
		}

		public static Task<HttpResponseMessage> PostDbRawAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary());
		}
		#endregion

		#region Http Get
		public static Task<DbWebApiResponse> GetDbAsJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.GetDbRawAsync(requestUri, inputParameters).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public static Task<DbWebApiResponse> GetDbAsJsonAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.GetDbRawAsync(requestUri, inputParameters, cancellationToken).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public static DbWebApiResponse GetDbAsJson(this HttpClient client, string requestUri, InputParameterDictionary inputParameters = null)
		{
			try
			{
				return client.GetDbAsJsonAsync(requestUri, inputParameters).Result;
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

		public static DbWebApiResponse GetDbAsJson(this HttpClient client, string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return client.GetDbAsJson(requestUri, new InputParameterDictionary(anonymousTypeInstanceAsInputParameters));
		}

		public static Task<HttpResponseMessage> GetDbRawAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return client.GetAsync(requestUri.SpliceInputParameters(inputParameters), cancellationToken);
		}

		public static Task<HttpResponseMessage> GetDbRawAsync(this HttpClient client, string requestUri, InputParameterDictionary inputParameters)
		{
			return client.GetAsync(requestUri.SpliceInputParameters(inputParameters));
		}

		private static string SpliceInputParameters(this string requestUri, InputParameterDictionary inputParameters)
		{
			if (inputParameters == null || inputParameters.Count == 0)
				return requestUri;

			char[] ps = new char[] { '?', '=' };
			StringBuilder uriWithJsonInput = new StringBuilder(requestUri);

			uriWithJsonInput.Append((requestUri.LastIndexOfAny(ps) == -1) ? "?" : "&");
			uriWithJsonInput.Append(JsonInputParameterName);
			uriWithJsonInput.Append("=");
			uriWithJsonInput.Append(Uri.EscapeDataString(JsonConvert.SerializeObject(inputParameters)));

			return uriWithJsonInput.ToString();
		}
		#endregion

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
