// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataBooster.DbWebApi
{
	public static partial class WebApiExtensions
	{
		#region QueryString Utilities

		public static IDictionary<string, string> GetQueryStringDictionary(this HttpRequestMessage request)
		{
			if (request == null)
				return null;

			IDictionary<string, string> queryStrings;

			if (_QueryStringCache.TryGetValue(request.RequestUri, out queryStrings))
				return queryStrings;

			queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			string strName, strValue;
			var queryNameValuePairs = request.GetQueryNameValuePairs();

			if (queryNameValuePairs == null)
				return queryStrings;

			foreach (var pair in queryNameValuePairs)
				if (pair.Value != null)
				{
					strName = pair.Key.Trim();
					strValue = pair.Value.Trim();

					if (strName.Length > 0 && strValue.Length > 0)
						queryStrings[strName] = strValue;
				}

			_QueryStringCache.TryAdd(request.RequestUri, queryStrings);

			return queryStrings;
		}

		internal static string GetQueryFileName(this IDictionary<string, string> queryStrings, string queryName, string filenameExtension)
		{
			string queryFileName = queryStrings.GetQueryParameterValue(queryName);

			if (!string.IsNullOrEmpty(queryFileName))
			{
				if (queryFileName[0] == '.' || queryFileName[0] == '"')
					queryFileName = queryFileName.TrimStart('.', '"');

				if (queryFileName.Length > 0 && queryFileName[queryFileName.Length - 1] == '"')
					queryFileName = queryFileName.TrimEnd('"');

				if (queryFileName.Length > 0)
				{
					int dotPos = queryFileName.LastIndexOf('.');

					if (dotPos < 0)
						return queryFileName + '.' + filenameExtension;

					if (dotPos > 0)
						if (dotPos == queryFileName.Length - 1)
							return queryFileName + filenameExtension;
						else
							return queryFileName;
				}
			}

			return "[save_as]." + filenameExtension;
		}

		internal static string GetQueryParameterValue(this IDictionary<string, string> queryStringDictionary, string parameterName)
		{
			if (queryStringDictionary == null || queryStringDictionary.Count == 0 || string.IsNullOrEmpty(parameterName))
				return null;

			string parameterValue;

			if (queryStringDictionary.TryGetValue(parameterName, out parameterValue))
				return parameterValue;
			else
				return null;
		}

		#endregion

		/// <summary>
		/// <para>Gather required parameters of database stored procedure/function either from body or from uri query string.</para>
		/// <para>1. From body is the first priority, the input parameters will only be gathered from body if the request has a body (JSON object) even though it contains none valid parameter;</para>
		/// <para>2. If the request has no message body, suppose all required input parameters were encapsulated as a JSON string into a special query string named "JsonInput";</para>
		/// <para>3. If none of above exist, any query string which name matched with stored procedure input parameter' name will be forwarded to database.</para>
		/// <para>See example at https://github.com/DataBooster/DbWebApi/blob/master/Examples/MyDbWebApi/Controllers/DbWebApiController.cs </para>
		/// </summary>
		/// <param name="request">The HTTP request. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="parametersFromBody">The parameters read from body. If not null, this method won't further try to gather input parameters from uri query string.</param>
		/// <returns>Final input parameters dictionary (name-value pairs) to pass to database call.</returns>
		public static IDictionary<string, object> GatherInputParameters(this HttpRequestMessage request, IDictionary<string, object> parametersFromBody)
		{
			return GatherInputParameters(request, parametersFromBody, DbWebApiOptions.QueryStringContract.JsonInputParameterName);
		}

		/// <summary>
		/// <para>Gather required parameters of database stored procedure/function either from body or from uri query string.</para>
		/// <para>1. From body is the first priority, the input parameters will only be gathered from body if the request has a body (JSON object) even though it contains none valid parameter;</para>
		/// <para>2. If the request has no message body, suppose all required input parameters were encapsulated as a JSON string into a special query string named "JsonInput";</para>
		/// <para>3. If none of above exist, any query string which name matched with stored procedure input parameter' name will be forwarded to database.</para>
		/// <para>See example at https://github.com/DataBooster/DbWebApi/blob/master/Examples/MyDbWebApi/Controllers/DbWebApiController.cs </para>
		/// </summary>
		/// <param name="request">The HTTP request. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="parametersFromBody">The parameters read from body. If not null, this method won't further try to gather input parameters from uri query string.</param>
		/// <param name="jsonInput">The special pre-arranged name in query string. Default as JsonInput.</param>
		/// <returns>Final input parameters dictionary (name-value pairs) to pass to database call.</returns>
		public static IDictionary<string, object> GatherInputParameters(this HttpRequestMessage request, IDictionary<string, object> parametersFromBody, string jsonInput)
		{
			if (parametersFromBody != null)
				return parametersFromBody;

			IDictionary<string, string> queryStringDictionary = request.GetQueryStringDictionary();
			string jsonInputString;

			if (queryStringDictionary.TryGetValue(jsonInput, out jsonInputString))
				return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonInputString);
			else
				return queryStringDictionary.ToDictionary(t => t.Key, t => t.Value as object, StringComparer.OrdinalIgnoreCase);
		}

		public static void BulkGatherInputParameters<T>(this HttpRequestMessage request, IList<T> listOfParametersFromBody, string jsonInput) where T : IDictionary<string, object>
		{
			if (listOfParametersFromBody == null || listOfParametersFromBody.Count == 0)
				return;

			IDictionary<string, object> fixedParametersFromUri = GatherInputParameters(request, null, jsonInput);

			if (fixedParametersFromUri != null && fixedParametersFromUri.Count > 0)
				foreach (IDictionary<string, object> batch in listOfParametersFromBody)
					foreach (KeyValuePair<string, object> fromUri in fixedParametersFromUri)
						if (!batch.ContainsKey(fromUri.Key))
							batch.Add(fromUri);
		}

		public static void BulkGatherInputParameters<T>(this HttpRequestMessage request, IList<T> listOfParametersFromBody) where T : IDictionary<string, object>
		{
			BulkGatherInputParameters(request, listOfParametersFromBody, DbWebApiOptions.QueryStringContract.JsonInputParameterName);
		}
	}
}
