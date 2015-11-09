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

		public static IDictionary<string, object> GetQueryStringDictionary(this HttpRequestMessage request)
		{
			if (request == null)
				return null;

			IDictionary<string, object> queryStrings;

			if (_QueryStringCache.TryGetValue(request.RequestUri, out queryStrings))
				return queryStrings;

			queryStrings = request.GetQueryNameValuePairs().NameValuePairsToDictionary();

			if (queryStrings == null)
				return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			_QueryStringCache.TryAdd(request.RequestUri, queryStrings);

			return queryStrings;
		}

		internal static IDictionary<string, object> NameValuePairsToDictionary<T>(this IEnumerable<KeyValuePair<string, T>> nameValuePairs)
		{
			if (nameValuePairs == null)
				return null;
			else
				return nameValuePairs.GroupBy(n => n.Key.Trim(), v => v.Value, StringComparer.OrdinalIgnoreCase).Where(n => n.Key.Length > 0)
					.ToDictionary(k => k.Key, v => (v.Count() == 1) ? (object)v.First() : (object)v.ToArray(), StringComparer.OrdinalIgnoreCase);
		}

		internal static string GetQueryFileName(this IDictionary<string, object> queryStrings, string queryName, string filenameExtension)
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

		internal static string GetQueryParameterValue(this IDictionary<string, object> queryStringDictionary, string parameterName)
		{
			if (queryStringDictionary == null || queryStringDictionary.Count == 0 || string.IsNullOrEmpty(parameterName))
				return null;

			string prefix = DbWebApiOptions.QueryStringContract.ReservedParameterPrefix;
			object parameterValue;

			if (prefix.Length > 0)
				if (queryStringDictionary.TryGetValue(prefix + parameterName, out parameterValue))
					return GetFirstStringFromObject(parameterValue);

			if (queryStringDictionary.TryGetValue(parameterName, out parameterValue))
				return GetFirstStringFromObject(parameterValue);
			else
				return null;
		}

		private static string GetFirstStringFromObject(object oValue)
		{
			if (oValue == null)
				return null;

			string strValue = oValue as string;
			if (strValue != null)
				return strValue;

			string[] strValues = oValue as string[];
			if (strValues != null)
				if (strValues.Length > 0)
					return strValues[0];
				else
					return null;

			return oValue.ToString();
		}

		#endregion

		/// <summary>
		/// <para>Gather required parameters of database stored procedure/function from body and from uri query string.</para>
		/// <para>1. From body is the first priority, the input parameters will be gathered from body if the request has a message body;</para>
		/// <para>2. Suppose all required input parameters were encapsulated as a JSON string into a special query string named "JsonInput";</para>
		/// <para>3. Any query string which name matched with stored procedure input parameter' name will be forwarded to database.</para>
		/// </summary>
		/// <param name="request">The HTTP request. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="parametersFromBody">The parameters read from body.</param>
		/// <returns>Final input parameters dictionary (name-value pairs) to pass to database call.</returns>
		public static IDictionary<string, object> GatherInputParameters(this HttpRequestMessage request, IDictionary<string, object> parametersFromBody)
		{
			return GatherInputParameters(request, parametersFromBody, DbWebApiOptions.QueryStringContract.JsonInputParameterName);
		}

		/// <summary>
		/// <para>Gather required parameters of database stored procedure/function from body and from uri query string.</para>
		/// <para>1. From body is the first priority, the input parameters will be gathered from body if the request has a message body;</para>
		/// <para>2. Suppose all required input parameters were encapsulated as a JSON string into a special query string named "JsonInput";</para>
		/// <para>3. Any query string which name matched with stored procedure input parameter' name will be forwarded to database.</para>
		/// </summary>
		/// <param name="request">The HTTP request. This is an extension method to HttpRequestMessage, when you use instance method syntax to call this method, omit this parameter.</param>
		/// <param name="parametersFromBody">The parameters read from body.</param>
		/// <param name="jsonInput">The special pre-arranged name in query string. Default as JsonInput.</param>
		/// <returns>Final input parameters dictionary (name-value pairs) to pass to database call.</returns>
		public static IDictionary<string, object> GatherInputParameters(this HttpRequestMessage request, IDictionary<string, object> parametersFromBody, string jsonInput)
		{
			IDictionary<string, object> queryStringDictionary = request.GetQueryStringDictionary();
			string jsonInputString = queryStringDictionary.GetQueryParameterValue(jsonInput);

			if (!string.IsNullOrWhiteSpace(jsonInputString))
				queryStringDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonInputString);

			if (parametersFromBody == null)
				return queryStringDictionary;

			if (queryStringDictionary == null)
				return parametersFromBody;

			foreach (KeyValuePair<string, object> fromUri in queryStringDictionary)
				if (!parametersFromBody.ContainsKey(fromUri.Key))
					parametersFromBody.Add(fromUri);			// As a supplement

			return parametersFromBody;
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
							batch.Add(fromUri);					// As a supplement
		}

		public static void BulkGatherInputParameters<T>(this HttpRequestMessage request, IList<T> listOfParametersFromBody) where T : IDictionary<string, object>
		{
			BulkGatherInputParameters(request, listOfParametersFromBody, DbWebApiOptions.QueryStringContract.JsonInputParameterName);
		}
	}
}
