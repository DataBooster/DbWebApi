// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Linq;

namespace DataBooster.DbWebApi.Client
{
	public class DbWebApiResponse
	{
		public JObject[][] ResultSets { get; set; }
		public JObject OutputParameters { get; set; }
		public object ReturnValue { get; set; }
	}
}
