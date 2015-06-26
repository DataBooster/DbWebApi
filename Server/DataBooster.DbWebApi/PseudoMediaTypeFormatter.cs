// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace DataBooster.DbWebApi
{
	class PseudoMediaTypeFormatter : MediaTypeFormatter
	{
		private static MediaTypeFormatter _ReferFormatter;

		public PseudoMediaTypeFormatter(MediaTypeFormatter referFormatter = null)
		{
			SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
			SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

			_ReferFormatter = referFormatter ?? new JsonMediaTypeFormatter();
		}

		public override bool CanReadType(Type type)
		{
			return false;
		}

		public override bool CanWriteType(Type type)
		{
			return true;
		}

		// This is only used for exception messages
		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
		{
			return _ReferFormatter.WriteToStreamAsync(type, value, writeStream, content, transportContext);
		}
	}
}
