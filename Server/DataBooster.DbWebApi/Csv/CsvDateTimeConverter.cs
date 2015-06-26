// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using CsvHelper.TypeConversion;

namespace DataBooster.DbWebApi.Csv
{
	public class CsvDateTimeConverter : DateTimeConverter
	{
		/// <param name="options">The options to use when converting.</param>
		/// <param name="value">The object to convert to a string.</param>
		/// <returns>The string representation of the object.</returns>
		public override string ConvertToString(TypeConverterOptions options, object value)
		{
			if (value is DateTime && string.IsNullOrEmpty(options.Format))
			{
				DateTime dt = (DateTime)value;
				string fmt;

				if (dt.Millisecond == 0)
					fmt = (dt.Date == dt) ? "yyyy-MM-dd" : "yyyy-MM-dd HH-mm-ss";
				else
					fmt = "yyyy-MM-dd HH-mm-ss.fff";

				return dt.ToString(fmt);
			}
			else
				return base.ConvertToString(options, value);
		}
	}
}
