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
		public static TimeSpan DerivedParametersCacheExpireInterval
		{
			get { return DerivedParametersCache.ExpireInterval; }
			set { DerivedParametersCache.ExpireInterval = value; }
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
