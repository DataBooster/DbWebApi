// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using DbParallel.DataAccess;
using DataBooster.DbWebApi.Csv;
using DataBooster.DbWebApi.Excel;
using DataBooster.DbWebApi.Jsonp;
using DataBooster.DbWebApi.Razor;

namespace DataBooster.DbWebApi
{
	public static class WebApiExtensions
	{
		private static Collection<IFormatPlug> _FormatPlugs;
		private static PseudoMediaTypeFormatter _PseudoFormatter;
		private static PseudoContentNegotiator _PseudoContentNegotiator;
		private static CacheDictionary<Uri, Dictionary<string, string>> _QueryStringCache;
		private static TimeSpan _QueryStringCacheLifetime;

		public static TimeSpan QueryStringCacheLifetime
		{
			get { return _QueryStringCacheLifetime; }
			set { _QueryStringCacheLifetime = value; }
		}

		static WebApiExtensions()
		{
			_FormatPlugs = new Collection<IFormatPlug>();
			_PseudoContentNegotiator = new PseudoContentNegotiator();
			_QueryStringCache = new CacheDictionary<Uri, Dictionary<string, string>>();
			_QueryStringCacheLifetime = TimeSpan.FromSeconds(180);
		}

		#region Registration
		public static void RegisterDbWebApi(this HttpConfiguration config, bool supportRazor = true, bool supportJsonp = true, bool supportXlsx = true, bool supportCsv = true)
		{
			DbWebApiOptions.DerivedParametersCacheExpireInterval = new TimeSpan(0, 15, 0);

			if (supportCsv)
				config.AddFormatPlug(new CsvFormatPlug());
			if (supportXlsx)
				config.AddFormatPlug(new XlsxFormatPlug());
			if (supportJsonp)
				config.Formatters.Add(new JsonpMediaTypeFormatter());
			if (supportRazor)
				config.Formatters.Add(new RazorMediaTypeFormatter());
		}

		public static void AddFormatPlug(this HttpConfiguration config, IFormatPlug formatPlug, string queryStringParameterName = null)
		{
			if (formatPlug == null)
				throw new ArgumentNullException("formatPlug");

			if (_FormatPlugs.Count == 0)
			{
				config.SupportMediaTypeShortMapping(queryStringParameterName);

				_PseudoFormatter = new PseudoMediaTypeFormatter(config.Formatters.JsonFormatter);
				config.Formatters.Add(_PseudoFormatter);
			}
			else
				if (_FormatPlugs.Contains(formatPlug))
					return;

			_FormatPlugs.Add(formatPlug);

			if (!_PseudoFormatter.SupportedMediaTypes.Contains(formatPlug.DefaultMediaType))
				_PseudoFormatter.SupportedMediaTypes.Add(formatPlug.DefaultMediaType);

			foreach (var mediaType in formatPlug.SupportedMediaTypes)
				if (!_PseudoFormatter.SupportedMediaTypes.Contains(mediaType))
					_PseudoFormatter.SupportedMediaTypes.Add(mediaType);

			_PseudoFormatter.AddMediaTypeMapping(formatPlug.FormatShortName, formatPlug.DefaultMediaType, queryStringParameterName);
		}

		public static void SupportMediaTypeShortMapping(this HttpConfiguration config, string queryStringParameterName = null)
		{
			if (string.IsNullOrEmpty(queryStringParameterName))
				queryStringParameterName = DbWebApiOptions.QueryStringContract.MediaTypeParameterName;

			config.Formatters.JsonFormatter.AddMediaTypeMapping("json", new MediaTypeHeaderValue("application/json"), queryStringParameterName);
			config.Formatters.XmlFormatter.AddMediaTypeMapping("xml", new MediaTypeHeaderValue("application/xml"), queryStringParameterName);
		}

		private static void AddMediaTypeMapping(this MediaTypeFormatter mediaTypeFormatter, string type, MediaTypeHeaderValue mediaType, string queryStringParameterName)
		{
			if (mediaTypeFormatter != null && !mediaTypeFormatter.MediaTypeMappings.Any(m => m.ExistMediaTypeMapping(type)))
			{
				if (string.IsNullOrEmpty(queryStringParameterName))
					queryStringParameterName = DbWebApiOptions.QueryStringContract.MediaTypeParameterName;

				mediaTypeFormatter.AddQueryStringMapping(queryStringParameterName, type, mediaType);
				mediaTypeFormatter.AddUriPathExtensionMapping(type, mediaType);
			}
		}

		private static bool ExistMediaTypeMapping(this MediaTypeMapping mediaTypeMapping, string mediaFormat)
		{
			QueryStringMapping qsMapping = mediaTypeMapping as QueryStringMapping;

			if (qsMapping != null && qsMapping.QueryStringParameterValue == mediaFormat)
				return true;

			UriPathExtensionMapping ueMapping = mediaTypeMapping as UriPathExtensionMapping;

			if (ueMapping != null && ueMapping.UriPathExtension == mediaFormat)
				return true;

			return false;
		}
		#endregion

		#region Negotiation
		internal static ContentNegotiationResult Negotiate(this HttpRequestMessage request)
		{
			HttpConfiguration configuration = request.GetConfiguration();
			IContentNegotiator contentNegotiator = configuration.Services.GetContentNegotiator();
			IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;

			return contentNegotiator.Negotiate(typeof(StoredProcedureResponse), request, formatters);
		}

		internal static Encoding NegotiateEncoding(this HttpRequestMessage request, MediaTypeFormatter negotiatedFormatter)
		{
			return (negotiatedFormatter == null) ? null : _PseudoContentNegotiator.NegotiateEncoding(request, negotiatedFormatter);
		}

		private static IFormatPlug MatchFormatPlug(MediaTypeHeaderValue mediaType)
		{
			if (mediaType == null)
				return null;

			foreach (IFormatPlug formatPlug in _FormatPlugs)
				if (formatPlug.DefaultMediaType.Equals(mediaType) || formatPlug.SupportedMediaTypes.Contains(mediaType))
					return formatPlug;

			foreach (IFormatPlug formatPlug in _FormatPlugs)
				if (formatPlug.DefaultMediaType.MediaType == mediaType.MediaType || formatPlug.SupportedMediaTypes.Any(m => m.MediaType == mediaType.MediaType))
					return formatPlug;

			return null;
		}
		#endregion

		/// <summary>
		/// ExecuteDbApi is the DbWebApi extension method to ApiController.
		/// See example at https://github.com/DataBooster/DbWebApi/blob/master/Examples/MyDbWebApi/Controllers/DbWebApiController.cs
		/// </summary>
		/// <param name="apiController">Your ApiController to invoke this extension method</param>
		/// <param name="sp">Specifies the fully qualified name of database stored procedure or function</param>
		/// <param name="parameters">Specifies required parameters as name-value pairs</param>
		/// <returns>A complete HttpResponseMessage contains result data returned by the database</returns>
		public static HttpResponseMessage ExecuteDbApi(this ApiController apiController, string sp, IDictionary<string, object> parameters)
		{
			try
			{
				var negotiationResult = apiController.Request.Negotiate();

				if (negotiationResult != null && negotiationResult.Formatter is PseudoMediaTypeFormatter)
				{
					Encoding negotiatedEncoding = apiController.Request.NegotiateEncoding((negotiationResult == null) ? null : negotiationResult.Formatter);
					IFormatPlug formatPlug = MatchFormatPlug(negotiationResult.MediaType);

					if (formatPlug != null)
						return formatPlug.Respond(apiController, sp, parameters, negotiationResult.MediaType, negotiatedEncoding);
				}

				using (DbContext dbContext = new DbContext())
				{
					dbContext.SetNamingConvention(apiController.Request.GetQueryStringDictionary());

					if (negotiationResult != null)
					{
						if (negotiationResult.Formatter is XmlMediaTypeFormatter)
							return apiController.Request.CreateResponse(HttpStatusCode.OK, new ResponseRoot(dbContext.ExecuteDbApi(sp, parameters)));
						else if (negotiationResult.Formatter is RazorMediaTypeFormatter)
							return apiController.Request.CreateResponse(HttpStatusCode.OK, new RazorContext(dbContext.ExecuteDbApi(sp, parameters), parameters));
					}

					return apiController.Request.CreateResponse(HttpStatusCode.OK, dbContext.ExecuteDbApi(sp, parameters));
				}
			}
			finally
			{
				_QueryStringCache.TryRemove(apiController.Request.RequestUri);
				_QueryStringCache.RemoveExpiredKeys(_QueryStringCacheLifetime);
			}
		}

		#region QueryString Utilities

		public static Dictionary<string, string> GetQueryStringDictionary(this HttpRequestMessage request)
		{
			if (request == null)
				return null;

			Dictionary<string, string> queryStrings;

			if (_QueryStringCache.TryGetValue(request.RequestUri, out queryStrings))
				return queryStrings;

			var queryNameValuePairs = request.GetQueryNameValuePairs();
			if (queryNameValuePairs == null)
				return null;

			queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string strName, strValue;

			foreach (var pair in queryNameValuePairs)
				if (pair.Value != null)
				{
					strName = pair.Key.Trim();
					strValue = pair.Value.Trim();

					if (strName.Length > 0 && strValue.Length > 0)
						queryStrings[strName] = strValue;
				}

			_QueryStringCache.TryAdd(request.RequestUri, queryStrings);

			return queryStrings;
		}

		internal static string GetQueryFileName(this Dictionary<string, string> queryStrings, string queryName, string filenameExtension)
		{
			string queryFileName = queryStrings.GetQueryParameterValue(queryName);

			if (!string.IsNullOrEmpty(queryFileName))
			{
				if (queryFileName[0] == '.' || queryFileName[0] == '"')
					queryFileName = queryFileName.TrimStart('.', '"');

				if (queryFileName.Length > 0 && queryFileName[queryFileName.Length - 1] == '"')
					queryFileName = queryFileName.TrimEnd('"');

				if (queryFileName.Length > 0)
				{
					int dotPos = queryFileName.LastIndexOf('.');

					if (dotPos < 0)
						return queryFileName + '.' + filenameExtension;

					if (dotPos > 0)
						if (dotPos == queryFileName.Length - 1)
							return queryFileName + filenameExtension;
						else
							return queryFileName;
				}
			}

			return "[save_as]." + filenameExtension;
		}

		internal static string GetQueryParameterValue(this Dictionary<string, string> queryStringDictionary, string parameterName)
		{
			if (queryStringDictionary == null || queryStringDictionary.Count == 0 || string.IsNullOrEmpty(parameterName))
				return null;

			string parameterValue;

			if (queryStringDictionary.TryGetValue(parameterName, out parameterValue))
				return parameterValue;
			else
				return null;
		}

		#endregion

		/// <summary>
		/// Gather required parameters of database stored procedure/function either from body or from uri query string.
		/// 1. From body is the first priority, the input parameters will only be gathered from body if the request has a body (JSON object) even though it contains none valid parameter;
		/// 2. If the request has no message body, suppose all required input parameters were encapsulated as a JSON string into a special query string named "JsonInput";
		/// 3. If none of above exist, any query string which name matched with stored procedure input parameter' name will be forwarded to database.
		/// See example at https://github.com/DataBooster/DbWebApi/blob/master/Examples/MyDbWebApi/Controllers/DbWebApiController.cs
		/// </summary>
		/// <param name="request">The HTTP request. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="parametersFromBody">The parameters read from body. If not null, this method won't further try to gather input parameters from uri query string.</param>
		/// <returns>Final input parameters dictionary (name-value pairs) to pass to database call.</returns>
		public static Dictionary<string, object> GatherInputParameters(this HttpRequestMessage request, Dictionary<string, object> parametersFromBody)
		{
			return GatherInputParameters(request, parametersFromBody, DbWebApiOptions.QueryStringContract.JsonInputParameterName);
		}

		/// <summary>
		/// Gather required parameters of database stored procedure/function either from body or from uri query string.
		/// 1. From body is the first priority, the input parameters will only be gathered from body if the request has a body (JSON object) even though it contains none valid parameter;
		/// 2. If the request has no message body, suppose all required input parameters were encapsulated as a JSON string into a special query string named "JsonInput";
		/// 3. If none of above exist, any query string which name matched with stored procedure input parameter' name will be forwarded to database.
		/// See example at https://github.com/DataBooster/DbWebApi/blob/master/Examples/MyDbWebApi/Controllers/DbWebApiController.cs
		/// </summary>
		/// <param name="request">The HTTP request. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="parametersFromBody">The parameters read from body. If not null, this method won't further try to gather input parameters from uri query string.</param>
		/// <param name="jsonInput">The special pre-arranged name in query string. Default as JsonInput.</param>
		/// <returns>Final input parameters dictionary (name-value pairs) to pass to database call.</returns>
		public static Dictionary<string, object> GatherInputParameters(this HttpRequestMessage request, Dictionary<string, object> parametersFromBody, string jsonInput)
		{
			if (parametersFromBody != null)
				return parametersFromBody;

			Dictionary<string, string> queryStringDictionary = request.GetQueryStringDictionary();
			string jsonInputString;

			if (queryStringDictionary.TryGetValue(jsonInput, out jsonInputString))
				return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonInputString);
			else
				return queryStringDictionary.ToDictionary(t => t.Key, t => t.Value as object, StringComparer.OrdinalIgnoreCase);
		}
	}
}
