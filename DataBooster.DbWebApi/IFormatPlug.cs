// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace DataBooster.DbWebApi
{
	public interface IFormatPlug
	{
		MediaTypeHeaderValue DefaultMediaType { get; }
		IEnumerable<MediaTypeHeaderValue> SupportedMediaTypes { get; }
		string FormatShortName { get; }

		HttpResponseMessage Respond(ApiController apiController, string sp, IDictionary<string, object> parameters,
			MediaTypeHeaderValue negotiatedMediaType, Encoding negotiatedEncoding);
	}
}
