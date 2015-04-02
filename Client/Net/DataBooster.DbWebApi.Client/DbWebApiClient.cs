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
	public class DbWebApiClient : IDisposable
	{
		const string _DefaultJsonInputParameterName = "JsonInput";
		private bool _CreatedInternalClient;

		#region Properties
		private HttpClient _HttpClient;
		public HttpClient HttpClient
		{
			get { return _HttpClient; }
		}

		private HttpMethod _HttpMethod = HttpMethod.Post;
		public HttpMethod HttpMethod
		{
			get
			{
				return _HttpMethod;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("HttpMethod");
				if (value != HttpMethod.Post && value != HttpMethod.Get)
					throw new ArgumentOutOfRangeException("HttpMethod", "Only accepts HttpMethod.Post or HttpMethod.Get");

				_HttpMethod = value;
			}
		}

		private string _JsonInputParameterName = _DefaultJsonInputParameterName;
		public string JsonInputParameterName
		{
			get { return _JsonInputParameterName; }
			set { _JsonInputParameterName = string.IsNullOrEmpty(value) ? _DefaultJsonInputParameterName : value; }
		}
		#endregion

		#region Constructors
		public DbWebApiClient(bool useDefaultCredentials = true)
		{
			if (useDefaultCredentials)
				_HttpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = useDefaultCredentials });
			else
				_HttpClient = new HttpClient();

			_CreatedInternalClient = true;
		}

		public DbWebApiClient(string baseAddress, bool useDefaultCredentials = true)
			: this(useDefaultCredentials)
		{
			if (!string.IsNullOrWhiteSpace(baseAddress))
				_HttpClient.BaseAddress = new Uri(baseAddress);
		}

		public DbWebApiClient(HttpClient httpClient, string baseAddress = null)
		{
			if (httpClient == null)
				throw new ArgumentNullException("httpClient");

			_HttpClient = httpClient;

			if (!string.IsNullOrWhiteSpace(baseAddress))
				_HttpClient.BaseAddress = new Uri(baseAddress);

			_CreatedInternalClient = false;
		}
		#endregion

		#region ExecAsJson overrides
		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return ExecRawAsync(requestUri, inputParameters, cancellationToken).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsJsonAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters), cancellationToken);
		}

		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, InputParameterDictionary inputParameters = null)
		{
			return ExecRawAsync(requestUri, inputParameters).
				ContinueWith<DbWebApiResponse>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbJson();
				});
		}

		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsJsonAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
		}

		public DbWebApiResponse ExecAsJson(string requestUri, InputParameterDictionary inputParameters = null)
		{
			try
			{
				return ExecAsJsonAsync(requestUri, inputParameters).Result;
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

		public DbWebApiResponse ExecAsJson(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsJson(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
		}
		#endregion

		#region Raw Methods
		public Task<HttpResponseMessage> ExecRawAsync(string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			if (_HttpMethod == HttpMethod.Get)
				return GetRawAsync(requestUri, inputParameters, cancellationToken);
			else
				return PostRawAsync(requestUri, inputParameters, cancellationToken);
		}

		public Task<HttpResponseMessage> ExecRawAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecRawAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters), cancellationToken);
		}

		public Task<HttpResponseMessage> ExecRawAsync(string requestUri, InputParameterDictionary inputParameters)
		{
			if (_HttpMethod == HttpMethod.Get)
				return GetRawAsync(requestUri, inputParameters);
			else
				return PostRawAsync(requestUri, inputParameters);
		}

		public Task<HttpResponseMessage> ExecRawAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecRawAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
		}

		protected InputParameterDictionary AsInputParameters(object anonymousTypeInstanceAsInputParameters)
		{
			InputParameterDictionary inputParameterDictionary = anonymousTypeInstanceAsInputParameters as InputParameterDictionary;

			return inputParameterDictionary ?? new InputParameterDictionary(anonymousTypeInstanceAsInputParameters);
		}

		protected Task<HttpResponseMessage> PostRawAsync(string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return _HttpClient.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken);
		}

		protected Task<HttpResponseMessage> PostRawAsync(string requestUri, InputParameterDictionary inputParameters)
		{
			return _HttpClient.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary());
		}

		protected Task<HttpResponseMessage> GetRawAsync(string requestUri, InputParameterDictionary inputParameters, CancellationToken cancellationToken)
		{
			return _HttpClient.GetAsync(SpliceInputParameters(requestUri, inputParameters), cancellationToken);
		}

		protected Task<HttpResponseMessage> GetRawAsync(string requestUri, InputParameterDictionary inputParameters)
		{
			return _HttpClient.GetAsync(SpliceInputParameters(requestUri, inputParameters));
		}

		private string SpliceInputParameters(string requestUri, InputParameterDictionary inputParameters)
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

		#region Dispose
		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _CreatedInternalClient && _HttpClient != null)
			{
				_HttpClient.Dispose();
				_HttpClient = null;
			}
		}
		#endregion
	}
}
