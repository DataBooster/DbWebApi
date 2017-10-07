// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using DataBooster.DbWebApi.Csv;
using DataBooster.DbWebApi.Excel;
using DataBooster.DbWebApi.Jsonp;
using DataBooster.DbWebApi.Razor;
using DataBooster.DbWebApi.Form;

namespace DataBooster.DbWebApi
{
	public static partial class WebApiExtensions
	{
		private const string _RegisteredPropertyKey = "[DataBooster.DbWebApi]:Registered";
		private static PseudoMediaTypeFormatter _PseudoFormatter;

		#region Registration
		public static void RegisterDbWebApi(this HttpConfiguration config, bool supportRazor = true, bool supportJsonp = true, bool supportXlsx = true, bool supportCsv = true, bool supportBson = true, bool supportFormUrlEncoded = true, bool supportMultipartForm = true)
		{
			if (config.Properties.ContainsKey(_RegisteredPropertyKey))
				throw new InvalidOperationException("Registered DbWebApi Repeatedly");

			DbWebApiOptions.DerivedParametersCacheExpireInterval = TimeSpan.FromDays(1);

			config.SupportMediaTypeShortMapping();
#if WEB_API2
			if (supportBson)
			{
				BsonMediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
				bsonFormatter.AddMediaTypeMapping("bson", BsonMediaTypeFormatter.DefaultMediaType, null);
				config.Formatters.Add(bsonFormatter);
			}
#endif
			if (supportCsv)
				config.AddFormatPlug(new CsvFormatPlug());
			if (supportXlsx)
				config.AddFormatPlug(new XlsxFormatPlug());
			if (supportJsonp)
				config.Formatters.Add(new JsonpMediaTypeFormatter());
			if (supportRazor)
				config.Formatters.Add(new RazorMediaTypeFormatter());
			if (supportFormUrlEncoded)
				WebFormUrlEncodedMediaTypeFormatter.ReplaceJQueryMvcFormUrlEncodedFormatter(config.Formatters);
			if (supportMultipartForm)
				config.Formatters.Add(new MultipartFormDataMediaTypeFormatter());

			config.Properties.TryAdd(_RegisteredPropertyKey, true);
		}

		public static void AddFormatPlug(this HttpConfiguration config, IFormatPlug formatPlug, string queryStringParameterName = null)
		{
			if (formatPlug == null)
				throw new ArgumentNullException("formatPlug");

			if (_FormatPlugs.Count == 0)
			{
				//	config.SupportMediaTypeShortMapping(queryStringParameterName);

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

		//	[Obsolete("This method is deprecated and will be removed from public in the next major release.", false)]
		private static void SupportMediaTypeShortMapping(this HttpConfiguration config, string queryStringParameterName = null)
		{
			if (string.IsNullOrEmpty(queryStringParameterName))
				queryStringParameterName = DbWebApiOptions.QueryStringContract.MediaTypeParameterName;

			config.Formatters.JsonFormatter.AddMediaTypeMapping("json", JsonMediaTypeFormatter.DefaultMediaType, queryStringParameterName);
			config.Formatters.XmlFormatter.AddMediaTypeMapping("xml", XmlMediaTypeFormatter.DefaultMediaType, queryStringParameterName);
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
	}
}
