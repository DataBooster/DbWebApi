// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
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
			return (type == typeof(RazorContext));
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

			string resultText;

			using (var isolatedRazor = IsolatedRazorEngineService.Create(razorContext.GetConfigCreator()))
			{
				string cacheKey = string.Format("x{0:X8}", razorContext.RazorTemplate.GetHashCode());
				object model = razorContext.Model;

				if (isolatedRazor.IsTemplateCached(cacheKey, null))
					resultText = isolatedRazor.Run(cacheKey, null, model);
				else
					resultText = isolatedRazor.RunCompile(razorContext.RazorTemplate, cacheKey, null, model);
			}

			if (string.IsNullOrEmpty(resultText))
				return;

			var encoding = SelectCharacterEncoding(content == null ? null : content.Headers);

			using (var writer = new StreamWriter(writeStream, encoding))
			{
				writer.Write(resultText);
				writer.Flush();
			}
		}
	}
}
