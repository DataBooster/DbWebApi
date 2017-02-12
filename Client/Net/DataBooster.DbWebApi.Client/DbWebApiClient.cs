// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
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
	public partial class DbWebApiClient : IDisposable
	{
		const string _DefaultJsonInputParameterName = "JsonInput";
		private bool _CreatedInternalClient;

		#region Properties
		private HttpClient _HttpClient;
		public HttpClient HttpClient
		{
			get { return _HttpClient; }
		}

		public HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> AcceptMediaTypes
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

		#region Exec as StoredProcedureResponse overloads
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
			return ExecAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public Task<StoredProcedureResponse> ExecAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsync(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}

		public StoredProcedureResponse Exec(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<StoredProcedureResponse>(requestUri, inputParameters, cancellationToken);
		}

		public StoredProcedureResponse Exec(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return Exec(requestUri, inputParameters, CancellationToken.None);
		}

		public StoredProcedureResponse Exec(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<StoredProcedureResponse>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public StoredProcedureResponse Exec(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return Exec(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}
		#endregion

		#region Bulk Exec as StoredProcedureResponse overloads
		public Task<StoredProcedureResponse[]> ExecAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAsAsync<StoredProcedureResponse[]>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public Task<StoredProcedureResponse[]> ExecAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsync(requestUri, bulkInputParameterSets, CancellationToken.None);
		}

		public StoredProcedureResponse[] Exec<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAs<StoredProcedureResponse[]>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public StoredProcedureResponse[] Exec<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return Exec(requestUri, bulkInputParameterSets, CancellationToken.None);
		}

		#endregion

		#region ExecAsJson overloads
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
			return ExecAsJsonAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public Task<JObject> ExecAsJsonAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsJsonAsync(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}

		public JObject ExecAsJson(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<JObject>(requestUri, inputParameters, cancellationToken);
		}

		public JObject ExecAsJson(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsJson(requestUri, inputParameters, CancellationToken.None);
		}

		public JObject ExecAsJson(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<JObject>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public JObject ExecAsJson(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsJson(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}
		#endregion

		#region Bulk ExecAsJson overloads
		public Task<JObject[]> ExecAsJsonAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAsAsync<JObject[]>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public Task<JObject[]> ExecAsJsonAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsJsonAsync(requestUri, bulkInputParameterSets, CancellationToken.None);
		}

		public JObject[] ExecAsJson<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAs<JObject[]>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public JObject[] ExecAsJson<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsJson(requestUri, bulkInputParameterSets, CancellationToken.None);
		}
		#endregion

		#region ExecAsXml overloads
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
			return ExecAsXmlAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public Task<XDocument> ExecAsXmlAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsXmlAsync(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}

		public XDocument ExecAsXml(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<XDocument>(requestUri, inputParameters, cancellationToken);
		}

		public XDocument ExecAsXml(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsXml(requestUri, inputParameters, CancellationToken.None);
		}

		public XDocument ExecAsXml(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<XDocument>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public XDocument ExecAsXml(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsXml(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}
		#endregion

		#region Bulk ExecAsXml overloads
		public Task<XDocument> ExecAsXmlAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAsAsync<XDocument>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public Task<XDocument> ExecAsXmlAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsXmlAsync(requestUri, bulkInputParameterSets, CancellationToken.None);
		}

		public XDocument ExecAsXml<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAs<XDocument>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public XDocument ExecAsXml<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsXml(requestUri, bulkInputParameterSets, CancellationToken.None);
		}

		#endregion

		#region ExecAsString overloads
		public Task<string> ExecAsStringAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<string>(requestUri, inputParameters, cancellationToken);
		}

		public Task<string> ExecAsStringAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<string>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public Task<string> ExecAsStringAsync(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsStringAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public Task<string> ExecAsStringAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsStringAsync(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}

		public string ExecAsString(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<string>(requestUri, inputParameters, cancellationToken);
		}

		public string ExecAsString(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<string>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public string ExecAsString(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsString(requestUri, inputParameters, CancellationToken.None);
		}

		public string ExecAsString(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsString(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}
		#endregion

		#region Bulk ExecAsString overloads
		public Task<string> ExecAsStringAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAsAsync<string>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public Task<string> ExecAsStringAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsStringAsync<T>(requestUri, bulkInputParameterSets, CancellationToken.None);
		}

		public string ExecAsString<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAs<string>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public string ExecAsString<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsString<T>(requestUri, bulkInputParameterSets, CancellationToken.None);
		}
		#endregion

		#region ExecAsStream overloads
		public Task<Stream> ExecAsStreamAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<Stream>(requestUri, inputParameters, cancellationToken);
		}

		public Task<Stream> ExecAsStreamAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAsAsync<Stream>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public Task<Stream> ExecAsStreamAsync(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsStreamAsync(requestUri, inputParameters, CancellationToken.None);
		}

		public Task<Stream> ExecAsStreamAsync(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsStreamAsync(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}

		public Stream ExecAsStream(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<Stream>(requestUri, inputParameters, cancellationToken);
		}

		public Stream ExecAsStream(string requestUri, IDictionary<string, object> inputParameters = null)
		{
			return ExecAsStream(requestUri, inputParameters, CancellationToken.None);
		}

		public Stream ExecAsStream(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecAs<Stream>(requestUri, anonymousTypeInstanceAsInputParameters, cancellationToken);
		}

		public Stream ExecAsStream(string requestUri, object anonymousTypeInstanceAsInputParameters)
		{
			return ExecAsStream(requestUri, anonymousTypeInstanceAsInputParameters, CancellationToken.None);
		}
		#endregion

		#region Bulk ExecAsStream overloads
		public Task<Stream> ExecAsStreamAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAsAsync<Stream>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public Task<Stream> ExecAsStreamAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsStreamAsync<T>(requestUri, bulkInputParameterSets, CancellationToken.None);
		}

		public Stream ExecAsStream<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken)
		{
			return BulkExecAs<Stream>(requestUri, AsBulkInputParameterSets(bulkInputParameterSets), cancellationToken);
		}

		public Stream ExecAsStream<T>(string requestUri, ICollection<T> bulkInputParameterSets)
		{
			return ExecAsStream<T>(requestUri, bulkInputParameterSets, CancellationToken.None);
		}
		#endregion

		#region Exec as Generic overloads
#if WEB_API2
		protected async Task<T> ExecAsAsync<T>(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken) where T : class
		{
			HttpResponseMessage httpResponse = await ExecRawAsync(requestUri, inputParameters, cancellationToken).ConfigureAwait(false);
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

		protected T ExecAs<T>(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken) where T : class
		{
			try
			{
				return ExecAsAsync<T>(requestUri, inputParameters, cancellationToken).Result;
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

		protected T ExecAs<T>(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken) where T : class
		{
			return ExecAs<T>(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters), cancellationToken);
		}
		#endregion

		#region Bulk Exec as Generic overloads
#if WEB_API2
		protected async Task<T> BulkExecAsAsync<T>(string requestUri, ICollection<IDictionary<string, object>> bulkInputParameterSets, CancellationToken cancellationToken) where T : class
		{
			HttpResponseMessage httpResponse = await BulkExecRawAsync(requestUri, bulkInputParameterSets, cancellationToken).ConfigureAwait(false);
			return httpResponse.ReadAs<T>();
		}
#else	// ASP.NET Web API 1
		protected Task<T> BulkExecAsAsync<T>(string requestUri, ICollection<IDictionary<string, object>> bulkInputParameterSets, CancellationToken cancellationToken) where T : class
		{
			return BulkExecRawAsync(requestUri, bulkInputParameterSets, cancellationToken).
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

		protected T BulkExecAs<T>(string requestUri, ICollection<IDictionary<string, object>> bulkInputParameterSets, CancellationToken cancellationToken) where T : class
		{
			try
			{
				return BulkExecAsAsync<T>(requestUri, bulkInputParameterSets, cancellationToken).Result;
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
		protected Task<HttpResponseMessage> ExecRawAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			if (_HttpMethod == HttpMethod.Get)
				return GetRawAsync(requestUri, inputParameters, cancellationToken);
			else
				return PostRawAsync(requestUri, inputParameters, cancellationToken);
		}

		protected Task<HttpResponseMessage> ExecRawAsync(string requestUri, object anonymousTypeInstanceAsInputParameters, CancellationToken cancellationToken)
		{
			return ExecRawAsync(requestUri, AsInputParameters(anonymousTypeInstanceAsInputParameters), cancellationToken);
		}

		protected Task<HttpResponseMessage> PostRawAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return _HttpClient.PostAsJsonAsync(requestUri, inputParameters ?? new InputParameterDictionary(), cancellationToken);
		}

		protected Task<HttpResponseMessage> GetRawAsync(string requestUri, IDictionary<string, object> inputParameters, CancellationToken cancellationToken)
		{
			return _HttpClient.GetAsync(SpliceInputParameters(requestUri, inputParameters), cancellationToken);
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

		private IDictionary<string, object> AsInputParameters(object anonymousTypeInstanceAsInputParameters)
		{
			IDictionary<string, object> inputParameterDictionary = anonymousTypeInstanceAsInputParameters as IDictionary<string, object>;

			return inputParameterDictionary ?? new InputParameterDictionary(anonymousTypeInstanceAsInputParameters);
		}
		#endregion

		#region Bulk Raw Methods
		private const string _ErrBulkMethodGet = "Bulk Exec does not support HTTP GET";

		protected Task<HttpResponseMessage> BulkExecRawAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			if (_HttpMethod == HttpMethod.Get)
				throw new NotSupportedException(_ErrBulkMethodGet);
			else
				return BulkPostRawAsync(requestUri, bulkInputParameterSets, cancellationToken);
		}

		private Task<HttpResponseMessage> BulkPostRawAsync<T>(string requestUri, ICollection<T> bulkInputParameterSets, CancellationToken cancellationToken) where T : IDictionary<string, object>
		{
			if (bulkInputParameterSets == null)
				throw new ArgumentNullException("bulkInputParameterSets");

			if (bulkInputParameterSets.Count == 0)
				return CreateEmptyJsonArrayResponse();

			return _HttpClient.PostAsJsonAsync(requestUri, bulkInputParameterSets, cancellationToken);
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

		private ICollection<IDictionary<string, object>> AsBulkInputParameterSets<T>(ICollection<T> bulkAnonymousTypeParameters)
		{
			ICollection<IDictionary<string, object>> bulkInputParameterDictionary = bulkAnonymousTypeParameters as ICollection<IDictionary<string, object>>;

			return bulkInputParameterDictionary ?? bulkAnonymousTypeParameters.Select(o => AsInputParameters(o)).ToList();
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
