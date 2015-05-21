// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataBooster.DbWebApi.Client
{
	public static class ClientHelper
	{
		public static IEnumerable<DbWebApiResponse> BulkReadDbJson(this HttpResponseMessage httpResponse)
		{
			var content = httpResponse.Content;
			var contentType = content.GetContentType();

			if (contentType != null && contentType.ToLower().EndsWith("/json") == false)
				throw new HttpRequestException("Response Content-Type is not JSON");

			if (httpResponse.IsSuccessStatusCode)
			{
				Task<IEnumerable<DbWebApiResponse>> readTask = content.ReadAsAsync<IEnumerable<DbWebApiResponse>>();
				IEnumerable<DbWebApiResponse> dbWebApiResponse = readTask.Result;

				if (readTask.IsFaulted)
					throw readTask.Exception;

				return dbWebApiResponse;
			}
			else
				throw httpResponse.CreateUnsuccessException();
		}

		public static DbWebApiResponse ReadDbJson(this HttpResponseMessage httpResponse)
		{
			var content = httpResponse.Content;
			var contentType = content.GetContentType();

			if (contentType != null && contentType.ToLower().EndsWith("/json") == false)
				throw new HttpRequestException("Response Content-Type is not JSON");

			if (httpResponse.IsSuccessStatusCode)
			{
				Task<DbWebApiResponse> readTask = content.ReadAsAsync<DbWebApiResponse>();
				DbWebApiResponse dbWebApiResponse = readTask.Result;

				if (readTask.IsFaulted)
					throw readTask.Exception;

				return dbWebApiResponse;
			}
			else
				throw httpResponse.CreateUnsuccessException();
		}

		private static HttpRequestException CreateUnsuccessException(this HttpResponseMessage httpResponse)
		{
			var errorDictionary = httpResponse.Content.ReadAsAsync<HttpErrorClient>().Result;

			if (errorDictionary.Count == 0)
				return new HttpRequestException(string.Format("{0} ({1})", (int)httpResponse.StatusCode, httpResponse.ReasonPhrase));
			else
				return new HttpResponseClientException(errorDictionary);
		}

		private static string GetContentType(this HttpContent content)
		{
			if (content == null)
				return null;
			if (content.Headers == null)
				return null;
			if (content.Headers.ContentType == null)
				return null;
			return content.Headers.ContentType.MediaType;
		}

		public static IDictionary<string, Array> SeparateArrayByProperties<T>(this ICollection<T> sourceRows) where T : IDictionary<string, object>
		{
			if (sourceRows == null)
				throw new ArgumentNullException("sourceRows");

			int i = 0, size = sourceRows.Count;
			IDictionary<string, Type> propTypeDict = PreparePropertyType(sourceRows);
			Dictionary<string, Array> propValueDict = new Dictionary<string, Array>();

			foreach (var prop in propTypeDict)
				propValueDict.Add(prop.Key, Array.CreateInstance(prop.Value, size));

			foreach (var row in sourceRows)
			{
				foreach (var prop in propTypeDict)
					propValueDict[prop.Key].SetValue(Convert.ChangeType(row[prop.Key], prop.Value), i);

				i++;
			}

			return propValueDict;
		}

		private static IDictionary<string, Type> PreparePropertyType<T>(ICollection<T> sourceRows) where T : IDictionary<string, object>
		{
			Dictionary<string, Type> propTypeDict = new Dictionary<string, Type>();
			Dictionary<string, int> pendingProperties = new Dictionary<string, int>();

			foreach (var row in sourceRows)
			{
			}

			return propTypeDict;
		}
	}
}
