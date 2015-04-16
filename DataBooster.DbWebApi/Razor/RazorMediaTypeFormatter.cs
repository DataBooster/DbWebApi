// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using DbParallel.DataAccess;
using RazorEngine.Templating;

namespace DataBooster.DbWebApi.Razor
{
	public class RazorMediaTypeFormatter : BufferedMediaTypeFormatter
	{
		const string _FormatShortName = "razor";
		private static readonly MediaTypeHeaderValue _TextRazor = new MediaTypeHeaderValue("text/razor");
		private static readonly MediaTypeHeaderValue _ApplicationRazor = new MediaTypeHeaderValue("application/razor");

		public RazorMediaTypeFormatter()
		{
			SupportedMediaTypes.Add(_TextRazor);
			SupportedMediaTypes.Add(_ApplicationRazor);

			SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
			SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

			MediaTypeMappings.Add(new QueryStringMapping(DbWebApiOptions.QueryStringContract.MediaTypeParameterName, _FormatShortName, _TextRazor));
			MediaTypeMappings.Add(new UriPathExtensionMapping(_FormatShortName, _TextRazor));
		}

		public static MediaTypeHeaderValue DefaultMediaType
		{
			get { return _TextRazor; }
		}

		public override bool CanReadType(Type type)
		{
			return false;
		}

		public override bool CanWriteType(Type type)
		{
			return (type == typeof(RazorContext) || type == typeof(StoredProcedureResponse));
		}

		/// <param name="type">The type of the object to serialize.</param>
		/// <param name="value">The object value to write. Can be null.</param>
		/// <param name="writeStream">The stream to which to write.</param>
		/// <param name="content">The <see cref="T:System.Net.Http.HttpContent"/>, if available. Can be null.</param>
		public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
		{
			if (type != typeof(RazorContext))
				throw new ArgumentOutOfRangeException("type");
			if (writeStream == null)
				throw new ArgumentNullException("writeStream");

			RazorContext razorContext = value as RazorContext;
			if (razorContext == null)
				throw new ArgumentException("value is not RazorContext");

			Encoding encoding = SelectCharacterEncoding(content == null ? null : content.Headers);

			using (var isolatedRazor = IsolatedRazorEngineService.Create(razorContext.GetConfigCreator()))
			using (var writer = new StreamWriter(writeStream, encoding))
			{
				string cacheKey = razorContext.RazorTemplate.GetHashCode().ToString("X8");
				object model = razorContext.Model;

				isolatedRazor.RunCompile(razorContext.RazorTemplate, cacheKey, writer, null, model);
				writer.Flush();
			}
		}
	}
}
