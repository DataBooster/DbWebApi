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
using DbParallel.DataAccess;
using DataBooster.DbWebApi.Csv;
using DataBooster.DbWebApi.Excel;

namespace DataBooster.DbWebApi
{
	public static class WebApiExtensions
	{
		public const string DefaultQueryStringMediaTypeParameterName = "format";
		private static Collection<IFormatPlug> _FormatPlugs;
		private static PseudoMediaTypeFormatter _PseudoFormatter;
		private static PseudoContentNegotiator _PseudoContentNegotiator;

		static WebApiExtensions()
		{
			_FormatPlugs = new Collection<IFormatPlug>();
			_PseudoContentNegotiator = new PseudoContentNegotiator();
		}

		public static void RegisterDbWebApi(this HttpConfiguration config)
		{
			config.AddFormatPlug(new CsvFormatPlug());
			config.AddFormatPlug(new XlsxFormatPlug());
			DbWebApiOptions.DerivedParametersCacheExpireInterval = new TimeSpan(0, 15, 0);
		}

		public static void AddFormatPlug(this HttpConfiguration config, IFormatPlug formatPlug, string queryStringParameterName = DefaultQueryStringMediaTypeParameterName)
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

		public static void SupportMediaTypeShortMapping(this HttpConfiguration config, string queryStringParameterName = DefaultQueryStringMediaTypeParameterName)
		{
			config.Formatters.JsonFormatter.AddMediaTypeMapping("json", new MediaTypeHeaderValue("application/json"), queryStringParameterName);
			config.Formatters.XmlFormatter.AddMediaTypeMapping("xml", new MediaTypeHeaderValue("application/xml"), queryStringParameterName);
		}

		private static void AddMediaTypeMapping(this MediaTypeFormatter mediaTypeFormatter, string type, MediaTypeHeaderValue mediaType, string queryStringParameterName)
		{
			if (mediaTypeFormatter != null && !mediaTypeFormatter.MediaTypeMappings.Any(m => m.ExistMediaTypeMapping(type)))
			{
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

		/// <summary>
		/// ExecuteDbApi is the DbWebApi extension method to ApiController, 
		/// </summary>
		/// <param name="apiController">Your ApiController to invoke this extension method</param>
		/// <param name="sp">Specifies the fully qualified name of database stored procedure or function</param>
		/// <param name="parameters">Specifies required parameters as name-value pairs</param>
		/// <returns>A complete HttpResponseMessage contains result data returned by the database.</returns>
		public static HttpResponseMessage ExecuteDbApi(this ApiController apiController, string sp, IDictionary<string, object> parameters)
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
				return apiController.Request.CreateResponse(HttpStatusCode.OK, dbContext.ExecuteDbApi(sp, parameters));
			}
		}

		public static Dictionary<string, string> GetQueryStringDictionary(this HttpRequestMessage request)
		{
			if (request == null)
				return null;

			var queryNameValuePairs = request.GetQueryNameValuePairs();
			if (queryNameValuePairs == null)
				return null;

			Dictionary<string, string> queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string strName, strValue;

			foreach (var pair in queryNameValuePairs)
				if (pair.Value != null)
				{
					strName = pair.Key.Trim();
					strValue = pair.Value.Trim();

					if (strName.Length > 0 && strValue.Length > 0)
						queryStrings[strName] = strValue;
				}

			return queryStrings;
		}

		internal static string GetQueryFileName(this Dictionary<string, string> queryStrings, string queryName, string filenameExtension)
		{
			if (queryStrings != null && queryStrings.Count > 0)
			{
				string queryFileName;

				if (queryStrings.TryGetValue(queryName, out queryFileName))
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
			}

			return "[save_as]." + filenameExtension;
		}
	}
}
