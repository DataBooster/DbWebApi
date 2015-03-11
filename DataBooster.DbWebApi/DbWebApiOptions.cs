// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
		}

		public static TimeSpan DerivedParametersCacheExpireInterval
		{
			get { return DerivedParametersCache.ExpireInterval; }
			set { DerivedParametersCache.ExpireInterval = value; }
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

		static DbWebApiOptions()
		{
			DerivedParametersCacheExpireInterval = new TimeSpan(0, 10, 0);

			_CsvConfiguration = new CsvConfiguration();

			SetCsvDateTimeConverter();
		}

		private static void SetCsvDateTimeConverter()
		{
			Type dt = typeof(DateTime);
			Type cvt = TypeConverterFactory.GetConverter(dt).GetType();

			if (cvt == typeof(DateTimeConverter) || cvt == typeof(DefaultTypeConverter))
				TypeConverterFactory.AddConverter(dt, new CsvDateTimeConverter());
		}
	}
}
