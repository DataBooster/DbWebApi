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
using Newtonsoft.Json.Linq;
using DbParallel.DataAccess;

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

		public HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> AcceptMediaType
		{
			get { return _HttpClient.DefaultRequestHeaders.Accept; }
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

		#region Bulk Exec as StoredProcedureResponse overrides
		public Task<StoredProcedureResponse[]> ExecAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			return ExecAsAsync<StoredProcedureResponse[], T>(requestUri, listOfInputParameters, cancellationToken);
		}

		public Task<StoredProcedureResponse[]> ExecAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<StoredProcedureResponse[]>(requestUri, listOfAnonymousTypeParameters, cancellationToken);
		}

		public Task<StoredProcedureResponse[]> ExecAsync<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAsAsync<StoredProcedureResponse[], T>(requestUri, listOfInputParameters, CancellationToken.None);
		}

		public Task<StoredProcedureResponse[]> ExecAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAsAsync<StoredProcedureResponse[]>(requestUri, listOfAnonymousTypeParameters);
		}

		public StoredProcedureResponse[] Exec<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAs<StoredProcedureResponse[], T>(requestUri, listOfInputParameters);
		}

		public StoredProcedureResponse[] Exec(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAs<StoredProcedureResponse[]>(requestUri, listOfAnonymousTypeParameters);
		}
		#endregion

		#region Exec as StoredProcedureResponse overrides
		public Task<StoredProcedureResponse> ExecAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<StoredProcedureResponse>(requestUri, inputParameters, cancellationToken);
		}

		public Task<StoredProcedureResponse> ExecAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<StoredProcedureResponse>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public Task<StoredProcedureResponse> ExecAsync(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsAsync<StoredProcedureResponse>(requestUri, inputParameters);
		}

		public Task<StoredProcedureResponse> ExecAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsAsync<StoredProcedureResponse>(requestUri, anonymousTypeInstanceAsInputParameters);
		}

		public StoredProcedureResponse Exec(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAs<StoredProcedureResponse>(requestUri, inputParameters);
		}

		public StoredProcedureResponse Exec(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAs<StoredProcedureResponse>(requestUri, anonymousTypeInstanceAsInputParameters);
		}
		#endregion

		#region Bulk ExecAsJson overrides
		public Task<JObject[]> ExecAsJsonAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			return ExecAsAsync<JObject[], T>(requestUri, listOfInputParameters, cancellationToken);
		}

		public Task<JObject[]> ExecAsJsonAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<JObject[]>(requestUri, listOfAnonymousTypeParameters, cancellationToken);
		}

		public Task<JObject[]> ExecAsJsonAsync<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAsAsync<JObject[], T>(requestUri, listOfInputParameters, CancellationToken.None);
		}

		public Task<JObject[]> ExecAsJsonAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAsAsync<JObject[]>(requestUri, listOfAnonymousTypeParameters);
		}

		public JObject[] ExecAsJson<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAs<JObject[], T>(requestUri, listOfInputParameters);
		}

		public JObject[] ExecAsJson(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAs<JObject[]>(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}
		#endregion

		#region ExecAsJson overrides
		public Task<JObject> ExecAsJsonAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<JObject>(requestUri, inputParameters, cancellationToken);
		}

		public Task<JObject> ExecAsJsonAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<JObject>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public Task<JObject> ExecAsJsonAsync(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsAsync<JObject>(requestUri, inputParameters);
		}

		public Task<JObject> ExecAsJsonAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsAsync<JObject>(requestUri, anonymousTypeInstanceAsInputParameters);
		}

		public JObject ExecAsJson(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAs<JObject>(requestUri, inputParameters);
		}

		public JObject ExecAsJson(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAs<JObject>(requestUri, anonymousTypeInstanceAsInputParameters);
		}
		#endregion

		#region Bulk ExecAsXml overrides
		public Task<XDocument> ExecAsXmlAsync<T>(string requestUri, ICollection<T> listOfInputParameters, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			return ExecAsAsync<XDocument, T>(requestUri, listOfInputParameters, cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<XDocument>(requestUri, listOfAnonymousTypeParameters, cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAsAsync<XDocument, T>(requestUri, listOfInputParameters);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAsAsync<XDocument>(requestUri, listOfAnonymousTypeParameters);
		}

		public XDocument ExecAsXml<T>(string requestUri, ICollection<T> listOfInputParameters) where T : IDictionary<string, object>
		{
			return ExecAs<XDocument>(requestUri, listOfInputParameters);
		}

		public XDocument ExecAsXml(string requestUri, ICollection<object> listOfAnonymousTypeParameters)
		{
			return ExecAs<XDocument>(requestUri, listOfAnonymousTypeParameters);
		}
		#endregion

		#region ExecAsXml overrides
		public Task<XDocument> ExecAsXmlAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<XDocument>(requestUri, inputParameters, cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<XDocument>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsAsync<XDocument>(requestUri, inputParameters);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsAsync<XDocument>(requestUri, anonymousTypeInstanceAsInputParameters);
		}

		public XDocument ExecAsXml(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAs<XDocument>(requestUri, inputParameters);
		}

		public XDocument ExecAsXml(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAs<XDocument>(requestUri, anonymousTypeInstanceAsInputParameters);
		}
		#endregion

		#region Bulk Exec as Generic overrides
#if WEB_API2
		protected async Task<TResult> ExecAsAsync<TResult, TDic>(string requestUri, ICollection<TDic> listOfInputParameters, CancellationToken cancellationToken)
			where TResult : class
			where TDic : IDictionary<string, object>
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, listOfInputParameters, cancellationToken);
			return httpResponse.ReadAs<TResult>();
		}
#else	// ASP.NET Web API 1
		protected Task<TResult> ExecAsAsync<TResult, TDic>(string requestUri, ICollection<TDic> listOfInputParameters, CancellationToken cancellationToken)
			where TResult : class
			where TDic : IDictionary<string, object>
		{
			return ExecRawAsync(requestUri, listOfInputParameters, cancellationToken).
				ContinueWith<TResult>(requestTask =>
				{
					if (requestTask.IsCanceled)
						return null;
					if (requestTask.IsFaulted)
						throw requestTask.Exception;

					return requestTask.Result.ReadAs<TResult>();
				});
		}
#endif

		protected Task<T> ExecAsAsync<T>(string requestUri, ICollection<object> listOfAnonymousTypeParameters, CancellationToken cancellationToken) where T : class
		{
			return ExecAsAsync<T>(requestUri, AsInputParameters(listOfAnonymousTypeParameters), cancellationToken);
		}

		protected Task<TResult> ExecAsAsync<TResult, TDic>(string requestUri, ICollection<TDic> listOfInputParameters)
			where TResult : class
			where TDic : IDictionary<string, object>
		{
			return ExecAsAsync<TResult>(requestUri, listOfInputParameters, CancellationToken.None);
		}

		protected Task<T> ExecAsAsync<T>(string requestUri, ICollection<object> listOfAnonymousTypeParameters) where T : class
		{
			return ExecAsAsync<T>(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}

		protected TResult ExecAs<TResult, TDic>(string requestUri, ICollection<TDic> listOfInputParameters)
			where TResult : class
			where TDic : IDictionary<string, object>
		{
			try
			{
				return ExecAsAsync<TResult>(requestUri, listOfInputParameters).Result;
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

		protected T ExecAs<T>(string requestUri, ICollection<object> listOfAnonymousTypeParameters) where T : class
		{
			return ExecAs<T>(requestUri, AsInputParameters(listOfAnonymousTypeParameters));
		}
		#endregion

		#region Exec as Generic overrides
#if WEB_API2
		protected async Task<T> ExecAsAsync<T>(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken) where T : class
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, inputParameters, cancellationToken);
			return httpResponse.ReadAs<T>();
		}
#else	// ASP.NET Web API 1
		protected Task<T> ExecAsAsync<T>(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken) where T : class
		{
			return ExecRawAsync(requestUri, inputParameters, cancellationToken).
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

		protected Task<T> ExecAsAsync<T>(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken) where T : class
		{
			return ExecAsAsync<T>(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters), cancellationToken);
		}

		protected Task<T> ExecAsAsync<T>(string requestUri, IDictionary<string, object> inputParameters = null) where T : class
		{
			return ExecAsAsync<T>(requestUri, inputParameters, CancellationToken.None);
		}

		protected Task<T> ExecAsAsync<T>(string requestUri, object anonymousTypeInstanceAsInputParameters) where T : class
		{
			return ExecAsAsync<T>(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
		}

		protected T ExecAs<T>(string requestUri, IDictionary<string, object> inputParameters = null) where T : class
		{
			try
			{
				return ExecAsAsync<T>(requestUri, inputParameters).Result;
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

		protected T ExecAs<T>(string requestUri, object anonymousTypeInstanceAsInputParameters) where T : class
		{
			return ExecAs<T>(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters));
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

			emptyResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(new StoredProcedureResponse[0]),
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
