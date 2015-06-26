// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System.Text;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace DataBooster.DbWebApi
{
	class PseudoContentNegotiator : DefaultContentNegotiator
	{
		public Encoding NegotiateEncoding(HttpRequestMessage request, MediaTypeFormatter formatter)
		{
			return base.SelectResponseCharacterEncoding(request, formatter);
		}
	}
}
