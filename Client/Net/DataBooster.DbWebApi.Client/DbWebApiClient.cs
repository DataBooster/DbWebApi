// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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

		public DbWebApiClient(string username, string password)
		{
			_HttpClient = new HttpClient();

			_HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password))));

			_CreatedInternalClient = true;
		}

		public DbWebApiClient(string baseAddress, string username, string password)
			: this(username, password)
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

		#region Bulk ExecAsJson overrides
#if WEB_API2
		public async Task<DbWebApiResponse[]> ExecAsJsonAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, listOfInputParameters, cancellationToken);
			return httpResponse.BulkReadDbJson();
		}
#else	// ASP.NET Web API 1
		public Task<DbWebApiResponse[]> ExecAsJsonAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			return ExecRawAsync(requestUri, listOfInputParameters, cancellationToken).
				ContinueWith<DbWebApiResponse[]>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.BulkReadDbJson();
				});
		}
#endif

		public Task<DbWebApiResponse[]> ExecAsJsonAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters, CancellationToken cancellationToken)
		{
			return ExecAsJsonAsync(requestUri, AsInputParameters(listOfAnonymousTypeParameters), cancellationToken);
		}

		public Task<DbWebApiResponse[]> ExecAsJsonAsync<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAsJsonAsync(requestUri, listOfInputParameters, CancellationToken.None);
		}

		public Task<DbWebApiResponse[]> ExecAsJsonAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAsJsonAsync(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}

		public DbWebApiResponse[] ExecAsJson<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			try
			{
				return ExecAsJsonAsync(requestUri, listOfInputParameters).Result;
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

		public DbWebApiResponse[] ExecAsJson(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAsJson(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}
		#endregion

		#region ExecAsJson overrides
#if WEB_API2
		public async Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, inputParameters, cancellationToken);
			return httpResponse.ReadDbJson();
		}
#else	// ASP.NET Web API 1
		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
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
#endif

		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsJsonAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters), cancellationToken);
		}

		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsJsonAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public Task<DbWebApiResponse> ExecAsJsonAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsJsonAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
		}

		public DbWebApiResponse ExecAsJson(string requestUri, IDictionary<string, object> inputParameters = null)
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

		#region Bulk ExecAsXml overrides
#if WEB_API2
		public async Task<XDocument> ExecAsXmlAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, listOfInputParameters, cancellationToken);
			return httpResponse.ReadDbXml(null);
		}
#else	// ASP.NET Web API 1
		public Task<XDocument> ExecAsXmlAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			return ExecRawAsync(requestUri, listOfInputParameters, cancellationToken).
				ContinueWith<XDocument>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbXml(null);
				});
		}
#endif
		public Task<XDocument> ExecAsXmlAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters, CancellationToken cancellationToken)
		{
			return ExecAsXmlAsync(requestUri, AsInputParameters(listOfAnonymousTypeParameters), cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAsXmlAsync(requestUri, listOfInputParameters, CancellationToken.None);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAsXmlAsync(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}

		public XDocument ExecAsXml<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			try
			{
				return ExecAsXmlAsync(requestUri, listOfInputParameters).Result;
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

		public XDocument ExecAsXml(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAsXml(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}
		#endregion

		#region ExecAsXml overrides
#if WEB_API2
		public async Task<XDocument> ExecAsXmlAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, inputParameters, cancellationToken);
			return httpResponse.ReadDbXml(null);
		}
#else	// ASP.NET Web API 1
		public Task<XDocument> ExecAsXmlAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecRawAsync(requestUri, inputParameters, cancellationToken).
				ContinueWith<XDocument>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadDbXml(null);
				});
		}
#endif

		public Task<XDocument> ExecAsXmlAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsXmlAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters), cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsXmlAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsXmlAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
		}

		public XDocument ExecAsXml(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			try
			{
				return ExecAsXmlAsync(requestUri, inputParameters).Result;
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

		public XDocument ExecAsXml(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsXml(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
		}
		#endregion

		#region Bulk Raw Methods
		private const string _ErrBulkMethodGet = "Bulk Exec does not support HTTP GET";

		protected Task<HttpResponseMessage> ExecRawAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			if (_HttpMethod == HttpMethod.Get)
				throw new NotSupportedException(_ErrBulkMethodGet);
			else
				return PostRawAsync(requestUri, listOfInputParameters, cancellationToken);
		}

		protected Task<HttpResponseMessage> ExecRawAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters, CancellationToken cancellationToken)
		{
			return ExecRawAsync(requestUri, AsInputParameters(listOfAnonymousTypeParameters), cancellationToken);
		}

		protected Task<HttpResponseMessage> ExecRawAsync<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			if (_HttpMethod == HttpMethod.Get)
				throw new NotSupportedException(_ErrBulkMethodGet);
			else
				return PostRawAsync(requestUri, listOfInputParameters);
		}

		protected Task<HttpResponseMessage> ExecRawAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecRawAsync(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}

		protected ICollection<IDictionary<string, object>> AsInputParameters(ICollection<object> listOfAnonymousTypeParameters)
		{
			ICollection<IDictionary<string, object>> listOfInputParameterDictionary = listOfAnonymousTypeParameters as ICollection<IDictionary<string, object>>;

			return listOfInputParameterDictionary ?? listOfAnonymousTypeParameters.Select(o => AsInputParameters(o)).ToList();
		}

		protected Task<HttpResponseMessage> PostRawAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			if (listOfInputParameters == null)
				throw new ArgumentNullException("listOfInputParameters");

			if (listOfInputParameters.Count == 0)
				return CreateEmptyJsonArrayResponse();

			return _HttpClient.PostAsJsonAsync(requestUri, listOfInputParameters, cancellationToken);
		}

		protected Task<HttpResponseMessage> PostRawAsync<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			if (listOfInputParameters == null)
				throw new ArgumentNullException("listOfInputParameters");

			if (listOfInputParameters.Count == 0)
				return CreateEmptyJsonArrayResponse();

			return _HttpClient.PostAsJsonAsync(requestUri, listOfInputParameters);
		}

		private Task<HttpResponseMessage> CreateEmptyJsonArrayResponse()
		{
			TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
			HttpResponseMessage emptyResponseMessage = new HttpResponseMessage();

			emptyResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new DbWebApiResponse[0]),
				new UTF8Encoding(false, true), "application/json");

			tcs.SetResult(emptyResponseMessage);

			return tcs.Task;
		}
		#endregion

		#region Raw Methods
		public Task<HttpResponseMessage> ExecRawAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
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

		public Task<HttpResponseMessage> ExecRawAsync(string requestUri, IDictionary<string, object> inputParameters)
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

		protected IDictionary<string, object> AsInputParameters(object anonymousTypeInstanceAsInputParameters)
		{
			IDictionary<string, object> inputParameterDictionary = anonymousTypeInstanceAsInputParameters as IDictionary<string, object>;

			return inputParameterDictionary ?? new InputParameterDictionary(anonymousTypeInstanceAsInputParameters);
		}

		protected Task<HttpResponseMessage> PostRawAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return _HttpClient.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken);
		}

		protected Task<HttpResponseMessage> PostRawAsync(string requestUri, IDictionary<string, object> inputParameters)
		{
			return _HttpClient.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary());
		}

		protected Task<HttpResponseMessage> GetRawAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return _HttpClient.GetAsync(SpliceInputParameters(requestUri, inputParameters), cancellationToken);
		}

		protected Task<HttpResponseMessage> GetRawAsync(string requestUri, IDictionary<string, object> inputParameters)
		{
			return _HttpClient.GetAsync(SpliceInputParameters(requestUri, inputParameters));
		}

		private string SpliceInputParameters(string requestUri, IDictionary<string, object> inputParameters)
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
