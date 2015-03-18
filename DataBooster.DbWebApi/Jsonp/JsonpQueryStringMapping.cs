// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;

namespace DataBooster.DbWebApi.Jsonp
{
	class JsonpQueryStringMapping : QueryStringMapping
	{
		public JsonpQueryStringMapping(string queryStringParameterName, MediaTypeHeaderValue mediaType)
			: base(queryStringParameterName, "*", mediaType)
		{
		}

		public JsonpQueryStringMapping(string queryStringParameterName, string mediaType)
			: base(queryStringParameterName, "*", mediaType)
		{
		}

		public override double TryMatchMediaType(HttpRequestMessage request)
		{
			return string.IsNullOrEmpty(GetCallback(request, QueryStringParameterName)) ? 0.0 : 1.0;
		}

		internal static string GetCallback(HttpRequestMessage request, string callbackQueryParameter)
		{
			var queryStrings = request.GetQueryNameValuePairs();

			if (queryStrings == null || string.IsNullOrEmpty(callbackQueryParameter))
				return null;

			return queryStrings.FirstOrDefault(p => p.Key.Equals(callbackQueryParameter, StringComparison.OrdinalIgnoreCase)).Value;
		}
	}
}
