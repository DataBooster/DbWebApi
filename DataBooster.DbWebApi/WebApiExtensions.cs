// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

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
using DbParallel.DataAccess;
using DataBooster.DbWebApi.Csv;
using DataBooster.DbWebApi.Excel;
using DataBooster.DbWebApi.Jsonp;
using DataBooster.DbWebApi.Razor;

namespace DataBooster.DbWebApi
{
	public static partial class WebApiExtensions
	{
		private static Collection<IFormatPlug> _FormatPlugs;
		private static PseudoMediaTypeFormatter _PseudoFormatter;
		private static PseudoContentNegotiator _PseudoContentNegotiator;
		private static CacheDictionary<Uri, IDictionary<string, string>> _QueryStringCache;
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
			_QueryStringCache = new CacheDictionary<Uri, IDictionary<string, string>>();
			_QueryStringCacheLifetime = TimeSpan.FromSeconds(180);
		}

		#region Registration
		public static void RegisterDbWebApi(this HttpConfiguration config, bool supportRazor = true, bool supportJsonp = true, bool supportXlsx = true, bool supportCsv = true)
		{
			DbWebApiOptions.DerivedParametersCacheExpireInterval = TimeSpan.FromMinutes(15);

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
		/// <para>ExecuteDbApi is the DbWebApi extension method to ApiController.</para>
		/// <para>See example at https://github.com/DataBooster/DbWebApi/blob/master/Examples/MyDbWebApi/Controllers/DbWebApiController.cs </para>
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
				CleanupCache(apiController.Request.RequestUri);
			}
		}

		/// <summary>
		/// Bulk execute a DbApi with a IList&lt;IDictionary&lt;string, object&gt;&gt; (a collection of input parameters collection)
		/// </summary>
		/// <typeparam name="T">IDictionary&lt;string, object&gt;</typeparam>
		/// <param name="apiController">Your ApiController to invoke this extension method</param>
		/// <param name="sp">Specifies the fully qualified name of database stored procedure or function</param>
		/// <param name="listOfParameters">Specifies a collection of required parameter dictionary for every call in the bulk execution</param>
		/// <returns>A complete HttpResponseMessage contains an array of every result data returned by the database</returns>
		public static HttpResponseMessage BulkExecuteDbApi<T>(this ApiController apiController, string sp, IList<T> listOfParameters) where T : IDictionary<string, object>
		{
			if (listOfParameters == null || listOfParameters.Count == 0)
				return apiController.Request.CreateResponse(HttpStatusCode.BadRequest);

			try
			{
				var negotiationResult = apiController.Request.Negotiate();

				if (negotiationResult != null && negotiationResult.Formatter is PseudoMediaTypeFormatter)
					return apiController.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);

				using (DbContext dbContext = new DbContext())
				{
					dbContext.SetNamingConvention(apiController.Request.GetQueryStringDictionary());

					if (negotiationResult != null)
					{
						if (negotiationResult.Formatter is RazorMediaTypeFormatter)
							return apiController.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
						else if (negotiationResult.Formatter is XmlMediaTypeFormatter)
						{
							ResponseRoot[] xmlResponses = new ResponseRoot[listOfParameters.Count];

							for (int i = 0; i < xmlResponses.Length; i++)
								xmlResponses[i] = new ResponseRoot(dbContext.ExecuteDbApi(sp, listOfParameters[i]));

							return apiController.Request.CreateResponse(HttpStatusCode.OK, xmlResponses.AsQueryable());
						}
					}

					StoredProcedureResponse[] jsonResponses = new StoredProcedureResponse[listOfParameters.Count];

					for (int i = 0; i < jsonResponses.Length; i++)
						jsonResponses[i] = dbContext.ExecuteDbApi(sp, listOfParameters[i]);

					return apiController.Request.CreateResponse(HttpStatusCode.OK, jsonResponses.AsQueryable());
				}
			}
			finally
			{
				CleanupCache(apiController.Request.RequestUri);
			}
		}

		private static void CleanupCache(Uri requestUri)
		{
			_QueryStringCache.TryRemove(requestUri);
			_QueryStringCache.RemoveExpiredKeys(_QueryStringCacheLifetime);
			SelfRecoverDerivedParametersCache();
		}
	}
}
