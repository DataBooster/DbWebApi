// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace DataBooster.DbWebApi.Excel
{
	public class XlsxFormatPlug : IFormatPlug
	{
		private readonly MediaTypeHeaderValue _DefaultMediaType;
		private readonly MediaTypeHeaderValue[] _SupportedMediaTypes;

		public XlsxFormatPlug()
		{
			_DefaultMediaType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			_SupportedMediaTypes = new MediaTypeHeaderValue[] { _DefaultMediaType,
				new MediaTypeHeaderValue("application/ms-excel"), new MediaTypeHeaderValue("application/xlsx") };
		}

		public MediaTypeHeaderValue DefaultMediaType
		{
			get { return _DefaultMediaType; }
		}

		public IEnumerable<MediaTypeHeaderValue> SupportedMediaTypes
		{
			get { return _SupportedMediaTypes; }
		}

		public string FormatShortName
		{
			get { return "xlsx"; }
		}

		public HttpResponseMessage Respond(ApiController apiController, string sp, IDictionary<string, object> parameters,
			MediaTypeHeaderValue negotiatedMediaType, Encoding negotiatedEncoding)
		{
			HttpResponseMessage csvResponse = apiController.Request.CreateResponse();


			csvResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "[save_as]." + FormatShortName };

			return csvResponse;
		}
	}


}
