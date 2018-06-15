// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace DataBooster.DbWebApi.Csv
{
	public class CsvDateTimeConverter : DateTimeConverter
	{
		private string _Format_Date = "yyyy-MM-dd";
		/// <summary>
		/// The format string for a date value with an accuracy of 1 day (DateTime.TimeOfDay.Ticks == 0). The default is "yyyy-MM-dd".
		/// </summary>
		public string Format_Date
		{
			get
			{
				return _Format_Date;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("Format_Date");

				_Format_Date = value;
			}
		}

		private string _Format_DateTimeSecond = "yyyy-MM-dd HH:mm:ss";
		/// <summary>
		/// The format string for a DateTime value with an accuracy of 1 second (DateTime.Millisecond == 0). The default is "yyyy-MM-dd HH:mm:ss".
		/// </summary>
		public string Format_DateTimeSecond
		{
			get
			{
				return _Format_DateTimeSecond;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("Format_DateTimeSecond");

				_Format_DateTimeSecond = value;
			}
		}

		private string _Format_DateTimeFractionalSecond = "yyyy-MM-dd HH:mm:ss.fff";
		/// <summary>
		/// The format string for a DateTime value with an accuracy of 1 millisecond (DateTime.Millisecond != 0). The default is "yyyy-MM-dd HH:mm:ss.fff".
		/// </summary>
		public string Format_DateTimeFractionalSecond
		{
			get
			{
				return _Format_DateTimeFractionalSecond;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("Format_DateTimeFractionalSecond");

				_Format_DateTimeFractionalSecond = value;
			}
		}

#if WEB_API2	// .NETFramework 4.5 (After CsvHelper 2.16.3) - https://joshclose.github.io/CsvHelper/change-log

		/// <param name="value">The object to convert to a string.</param>
		/// <param name="row">The <see cref="T:CsvHelper.IWriterRow"/> for the current record.</param>
		/// <param name="memberMapData">The <see cref="T:CsvHelper.Configuration.MemberMapData"/> for the member being written.</param>
		/// <returns>The string representation of the object.</returns>
		public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
		{
			var format = memberMapData.TypeConverterOptions.Formats;

			if (value is DateTime && (format == null || format.Length == 0 || string.IsNullOrEmpty(format[0])))
				return ConvertDateToString((DateTime)value);
			else
				return base.ConvertToString(value, row, memberMapData);
		}

#else			// .NETFramework 4.0 (Until CsvHelper 2.16.3)

		/// <param name="options">The options to use when converting.</param>
		/// <param name="value">The object to convert to a string.</param>
		/// <returns>The string representation of the object.</returns>
		public override string ConvertToString(TypeConverterOptions options, object value)
		{
			if (value is DateTime && string.IsNullOrEmpty(options.Format))
				return ConvertDateToString((DateTime)value);
			else
				return base.ConvertToString(options, value);
		}

#endif

		private string ConvertDateToString(DateTime value)
		{
			string fmt;

			if (value.Millisecond == 0)
				fmt = (value.TimeOfDay.Ticks == 0L) ? _Format_Date : _Format_DateTimeSecond;
			else
				fmt = _Format_DateTimeFractionalSecond;

			return value.ToString(fmt);
		}
	}
}
