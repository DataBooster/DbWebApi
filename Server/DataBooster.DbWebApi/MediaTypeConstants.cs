using System;
using System.Net.Http.Headers;

namespace DataBooster.DbWebApi
{
	internal static class MediaTypeConstants
	{
		private static readonly MediaTypeHeaderValue _ApplicationJsonMediaType = new MediaTypeHeaderValue("application/json");
		public static MediaTypeHeaderValue ApplicationJsonMediaType
		{
			get { return _ApplicationJsonMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _TextJsonMediaType = new MediaTypeHeaderValue("text/json");
		public static MediaTypeHeaderValue TextJsonMediaType
		{
			get { return _TextJsonMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationXmlMediaType = new MediaTypeHeaderValue("application/xml");
		public static MediaTypeHeaderValue ApplicationXmlMediaType
		{
			get { return _ApplicationXmlMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _TextXmlMediaType = new MediaTypeHeaderValue("text/xml");
		public static MediaTypeHeaderValue TextXmlMediaType
		{
			get { return _TextXmlMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationFormUrlEncodedMediaType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
		public static MediaTypeHeaderValue ApplicationFormUrlEncodedMediaType
		{
			get { return _ApplicationFormUrlEncodedMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _TextCsvMediaType = new MediaTypeHeaderValue("text/csv");
		public static MediaTypeHeaderValue TextCsvMediaType
		{
			get { return _TextCsvMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationVndOpenXmlSheetMediaType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		public static MediaTypeHeaderValue ApplicationVndOpenXmlSheetMediaType
		{
			get { return _ApplicationVndOpenXmlSheetMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationMsExcelMediaType = new MediaTypeHeaderValue("application/ms-excel");
		public static MediaTypeHeaderValue ApplicationMsExcelMediaType
		{
			get { return _ApplicationMsExcelMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationXlsxMediaType = new MediaTypeHeaderValue("application/xlsx");
		public static MediaTypeHeaderValue ApplicationXlsxMediaType
		{
			get { return _ApplicationXlsxMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationJavascriptMediaType = new MediaTypeHeaderValue("application/javascript");
		public static MediaTypeHeaderValue ApplicationJavascriptMediaType
		{
			get { return _ApplicationJavascriptMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationJsonpMediaType = new MediaTypeHeaderValue("application/json-p");
		public static MediaTypeHeaderValue ApplicationJsonpMediaType
		{
			get { return _ApplicationJsonpMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _TextJavascriptMediaType = new MediaTypeHeaderValue("text/javascript");
		public static MediaTypeHeaderValue TextJavascriptMediaType
		{
			get { return _TextJavascriptMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _TextRazorMediaType = new MediaTypeHeaderValue("text/razor");
		public static MediaTypeHeaderValue TextRazorMediaType
		{
			get { return _TextRazorMediaType.Clone(); }
		}

		private static readonly MediaTypeHeaderValue _ApplicationRazorMediaType = new MediaTypeHeaderValue("application/razor");
		public static MediaTypeHeaderValue ApplicationRazorMediaType
		{
			get { return _ApplicationRazorMediaType.Clone(); }
		} 

		internal static T Clone<T>(this T value) where T : ICloneable
		{
			return (T)value.Clone();
		}
	}
}
