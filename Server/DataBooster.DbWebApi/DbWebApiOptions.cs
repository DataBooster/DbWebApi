// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using DbParallel.DataAccess;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DataBooster.DbWebApi.Csv;

namespace DataBooster.DbWebApi
{
	public static class DbWebApiOptions
	{
		public static class QueryStringContract
		{
			const string _DefaultMediaTypeParameterName = "format";
			const string _DefaultNamingCaseParameterName = "NamingCase";
			const string _DefaultResultSetParameterName = "ResultSet";
			const string _DefaultFileNameParameterName = "FileName";
			const string _DefaultJsonInputParameterName = "JsonInput";
			const string _DefaultJsonpCallbackParameterName = "callback";
			const string _DefaultJsonpStateParameterName = "jsonpState";
			const string _DefaultRazorEncodingParameterName = "RazorEncoding";
			const string _DefaultRazorLanguageParameterName = "RazorLanguage";
			const string _DefaultRazorTemplateParameterName = "RazorTemplate";
			const string _DefaultXmlAsAttributeParameterName = "XmlAsAttribute";
			const string _DefaultXmlNullValueParameterName = "XmlNullValue";
			const string _DefaultXmlTypeSchemaParameterName = "XmlTypeSchema";

			private static string _ReservedParameterPrefix = string.Empty;
			public static string ReservedParameterPrefix
			{
				get { return _ReservedParameterPrefix; }
				set { _ReservedParameterPrefix = value ?? string.Empty; }
			}

			private static string _MediaTypeParameterName = _DefaultMediaTypeParameterName;
			public static string MediaTypeParameterName
			{
				get { return _MediaTypeParameterName; }
				set { _MediaTypeParameterName = string.IsNullOrEmpty(value) ? _DefaultMediaTypeParameterName : value; }
			}

			private static string _NamingCaseParameterName = _DefaultNamingCaseParameterName;
			public static string NamingCaseParameterName
			{
				get { return _NamingCaseParameterName; }
				set { _NamingCaseParameterName = string.IsNullOrEmpty(value) ? _DefaultNamingCaseParameterName : value; }
			}

			private static string _ResultSetParameterName = _DefaultResultSetParameterName;
			public static string ResultSetParameterName
			{
				get { return _ResultSetParameterName; }
				set { _ResultSetParameterName = string.IsNullOrEmpty(value) ? _DefaultResultSetParameterName : value; }
			}

			private static string _FileNameParameterName = _DefaultFileNameParameterName;
			public static string FileNameParameterName
			{
				get { return _FileNameParameterName; }
				set { _FileNameParameterName = string.IsNullOrEmpty(value) ? _DefaultFileNameParameterName : value; }
			}

			private static string _JsonInputParameterName = _DefaultJsonInputParameterName;
			public static string JsonInputParameterName
			{
				get { return _JsonInputParameterName; }
				set { _JsonInputParameterName = string.IsNullOrEmpty(value) ? _DefaultJsonInputParameterName : value; }
			}

			private static string _JsonpCallbackParameterName = _DefaultJsonpCallbackParameterName;
			public static string JsonpCallbackParameterName
			{
				get { return _JsonpCallbackParameterName; }
				set { _JsonpCallbackParameterName = string.IsNullOrEmpty(value) ? _DefaultJsonpCallbackParameterName : value; }
			}

			private static string _JsonpStateParameterName = _DefaultJsonpStateParameterName;
			public static string JsonpStateParameterName
			{
				get { return _JsonpStateParameterName; }
				set { _JsonpStateParameterName = string.IsNullOrEmpty(value) ? _DefaultJsonpStateParameterName : value; }
			}

			private static string _RazorEncodingParameterName = _DefaultRazorEncodingParameterName;
			public static string RazorEncodingParameterName
			{
				get { return _RazorEncodingParameterName; }
				set { _RazorEncodingParameterName = string.IsNullOrEmpty(value) ? _DefaultRazorEncodingParameterName : value; }
			}

			private static string _RazorLanguageParameterName = _DefaultRazorLanguageParameterName;
			public static string RazorLanguageParameterName
			{
				get { return _RazorLanguageParameterName; }
				set { _RazorLanguageParameterName = string.IsNullOrEmpty(value) ? _DefaultRazorLanguageParameterName : value; }
			}

			private static string _RazorTemplateParameterName = _DefaultRazorTemplateParameterName;
			public static string RazorTemplateParameterName
			{
				get { return _RazorTemplateParameterName; }
				set { _RazorTemplateParameterName = string.IsNullOrEmpty(value) ? _DefaultRazorTemplateParameterName : value; }
			}

			private static string _XmlAsAttributeParameterName = _DefaultXmlAsAttributeParameterName;
			public static string XmlAsAttributeParameterName
			{
				get { return _XmlAsAttributeParameterName; }
				set { _XmlAsAttributeParameterName = string.IsNullOrEmpty(value) ? _DefaultXmlAsAttributeParameterName : value; }
			}

			private static string _XmlNullValueParameterName = _DefaultXmlNullValueParameterName;
			public static string XmlNullValueParameterName
			{
				get { return _XmlNullValueParameterName; }
				set { _XmlNullValueParameterName = string.IsNullOrEmpty(value) ? _DefaultXmlNullValueParameterName : value; }
			}

			private static string _XmlTypeSchemaParameterName = _DefaultXmlTypeSchemaParameterName;
			public static string XmlTypeSchemaParameterName
			{
				get { return _XmlTypeSchemaParameterName; }
				set { _XmlTypeSchemaParameterName = string.IsNullOrEmpty(value) ? _DefaultXmlTypeSchemaParameterName : value; }
			}
		}

		public static class DetectDdlChangesContract
		{
			const string _DefaultCommaDelimitedSpListParameterName = "inCommaDelimitedString";
			const string _DefaultElapsedTimeParameterName = "inElapsedMinutes";

			private static string _CommaDelimitedSpListParameterName = _DefaultCommaDelimitedSpListParameterName;
			public static string CommaDelimitedSpListParameterName
			{
				get { return _CommaDelimitedSpListParameterName; }
				set { _CommaDelimitedSpListParameterName = string.IsNullOrEmpty(value) ? _DefaultCommaDelimitedSpListParameterName : value; }
			}

			private static string _ElapsedTimeParameterName = _DefaultElapsedTimeParameterName;
			public static string ElapsedTimeParameterName
			{
				get { return _ElapsedTimeParameterName; }
				set { _ElapsedTimeParameterName = string.IsNullOrEmpty(value) ? _DefaultElapsedTimeParameterName : value; }
			}

			private static TimeSpan _CacheExpireIntervalWithDetection = TimeSpan.FromDays(365);
			public static TimeSpan CacheExpireIntervalWithDetection
			{
				get { return _CacheExpireIntervalWithDetection; }
				set { if (value > _CacheExpireIntervalWithoutDetection) _CacheExpireIntervalWithDetection = value; }
			}

			private static TimeSpan _CacheExpireIntervalWithoutDetection;
			internal static TimeSpan CacheExpireIntervalWithoutDetection
			{
				get { return _CacheExpireIntervalWithoutDetection; }
				set { if (value < _CacheExpireIntervalWithDetection) _CacheExpireIntervalWithoutDetection = value; }
			}

			static DetectDdlChangesContract()
			{
				_CacheExpireIntervalWithoutDetection = DerivedParametersCache.ExpireInterval;
			}
		}

		public static TimeSpan DerivedParametersCacheExpireInterval
		{
			get
			{
				return DerivedParametersCache.ExpireInterval;
			}
			set
			{
				if (!WebApiExtensions.DerivedParametersCacheInPeriodicDetection)
				{
					DerivedParametersCache.ExpireInterval = value;
					DetectDdlChangesContract.CacheExpireIntervalWithoutDetection = value;
				}
			}
		}

		private static PropertyNamingConvention _DefaultPropertyNamingConvention = PropertyNamingConvention.None;
		public static PropertyNamingConvention DefaultPropertyNamingConvention
		{
			get { return _DefaultPropertyNamingConvention; }
			set { _DefaultPropertyNamingConvention = value; }
		}

		private static readonly CsvConfiguration _CsvConfiguration;
		public static CsvConfiguration CsvConfiguration
		{
			get { return _CsvConfiguration; }
		}

		private static readonly CsvDateTimeConverter _CsvDateTimeConverter;
		public static CsvDateTimeConverter CsvDateTimeConverter
		{
			get { return _CsvDateTimeConverter; }
		}

		static DbWebApiOptions()
		{
			_CsvConfiguration = new CsvConfiguration();

			#region	SetCsvDateTimeConverter();
			Type dt = typeof(DateTime);
			Type cvt = TypeConverterFactory.GetConverter(dt).GetType();

			if (cvt == typeof(DateTimeConverter) || cvt == typeof(DefaultTypeConverter))
			{
				_CsvDateTimeConverter = new CsvDateTimeConverter();
				TypeConverterFactory.AddConverter(dt, _CsvDateTimeConverter);
			}
			#endregion
		}

		private static RazorEngine.Encoding _DefaultRazorEncoding = RazorEngine.Encoding.Raw;
		public static RazorEngine.Encoding DefaultRazorEncoding
		{
			get { return _DefaultRazorEncoding; }
			set { _DefaultRazorEncoding = value; }
		}

		private static RazorEngine.Language _DefaultRazorLanguage = RazorEngine.Language.CSharp;
		public static RazorEngine.Language DefaultRazorLanguage
		{
			get { return _DefaultRazorLanguage; }
			set { _DefaultRazorLanguage = value; }
		}

		/*
		#region Database Connection
		public static DbProviderFactory DbProviderFactory
		{
			get { return ConfigHelper.DbProviderFactory; }
			set { ConfigHelper.DbProviderFactory = value; }
		}

		public static string ConnectionString
		{
			get { return ConfigHelper.ConnectionString; }
			set { ConfigHelper.ConnectionString = value; }
		}

		public static string ConnectionSettingKey
		{
			get { return ConfigHelper.ConnectionSettingKey; }
			set { ConfigHelper.ConnectionSettingKey = value; }
		}
		#endregion
		*/
	}
}
