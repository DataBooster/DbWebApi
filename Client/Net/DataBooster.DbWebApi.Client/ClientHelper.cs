// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DataBooster.DbWebApi.Client
{
	public static class ClientHelper
	{
		private static MediaTypeFormatterCollection _ReadAsMediaTypeFormatterCollection = null;

		public static MediaTypeFormatterCollection ReadAsMediaTypeFormatterCollection
		{
			get
			{
				if (_ReadAsMediaTypeFormatterCollection == null)
				{
					_ReadAsMediaTypeFormatterCollection = new MediaTypeFormatterCollection();
#if WEB_API2
					_ReadAsMediaTypeFormatterCollection.Insert(0, new BsonMediaTypeFormatter());
#endif
				}

				return _ReadAsMediaTypeFormatterCollection;
			}
		}

		#region Read response as JSON|BSON extentions

		internal static DbWebApiResponse[] BulkReadDbJson(this HttpResponseMessage httpResponse)
		{
			var content = httpResponse.Content;

			if (content == null)
				throw new ArgumentNullException("httpResponse.Content");

			if (httpResponse.IsSuccessStatusCode)
			{
				Task<DbWebApiResponse[]> readTask = content.ReadAsAsync<DbWebApiResponse[]>(ReadAsMediaTypeFormatterCollection);
				DbWebApiResponse[] dbWebApiResponse = readTask.Result;

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

			if (content == null)
				throw new ArgumentNullException("httpResponse.Content");

			if (httpResponse.IsSuccessStatusCode)
			{
				Task<DbWebApiResponse> readTask = content.ReadAsAsync<DbWebApiResponse>(ReadAsMediaTypeFormatterCollection);
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
			HttpErrorClient errorDictionary;

			try
			{
				errorDictionary = httpResponse.Content.ReadAsAsync<HttpErrorClient>(ReadAsMediaTypeFormatterCollection).Result;
			}
			catch
			{
				errorDictionary = null;
			}

			if (errorDictionary == null || errorDictionary.Count == 0)
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

		#endregion

		#region Read response as XML extentions

		public static XDocument ReadDbXml(this HttpResponseMessage httpResponse, string checkContentTypeEndsWith = "/xml")
		{
			var content = httpResponse.Content;

			if (content == null)
				throw new ArgumentNullException("httpResponse.Content");

			if (httpResponse.IsSuccessStatusCode)
			{
				if (!string.IsNullOrEmpty(checkContentTypeEndsWith))
				{
					var contentType = content.GetContentType();

					if (contentType != null && contentType.EndsWith(checkContentTypeEndsWith, StringComparison.OrdinalIgnoreCase) == false)
						throw new HttpRequestException("Response Content-Type is not XML");
				}

				Task<Stream> readTask = content.ReadAsStreamAsync();

				if (readTask.IsFaulted)
					throw readTask.Exception;

				return XDocument.Load(readTask.Result);
			}
			else
				throw httpResponse.CreateUnsuccessException();
		}

		#endregion

		#region SeparateArrayByProperties overloads

		/// <summary>
		/// Separate dictionary (IDictionary&lt;string, object&gt;) array by each key, to be a single Dictionary&lt;string, Array&gt;, every key has an array or values.
		/// </summary>
		/// <typeparam name="T">IDictionary&lt;string, object&gt;</typeparam>
		/// <param name="sourceRows">Source collection of dictionary, each dictionary must have the same keys.</param>
		/// <returns>A new Dictionary&lt;string, Array&gt;</returns>
		public static IDictionary<string, Array> SeparateArrayByProperties<T>(this ICollection<T> sourceRows) where T : IDictionary<string, object>
		{
			if (sourceRows == null)
				throw new ArgumentNullException("sourceRows");

			int i = 0, size = sourceRows.Count;
			Dictionary<string, Array> propArrayDict = new Dictionary<string, Array>();
			Array separateArray;

			foreach (var row in sourceRows)
			{
				if (i == 0)
					foreach (var prop in row)
						propArrayDict.Add(prop.Key, new object[size]);

				foreach (var prop in row)
					if (propArrayDict.TryGetValue(prop.Key, out separateArray))
						separateArray.SetValue(prop.Value, i);

				i++;
			}

			return propArrayDict;
		}

		/// <summary>
		/// Separate an array of anonymous type (or named type) instances by each property, to be a single Dictionary&lt;string, Array&gt;, every key has an array or values.
		/// </summary>
		/// <param name="anonymousTypeSourceRows">Source collection of anonymous type (or named type) instances</param>
		/// <returns>A new Dictionary&lt;string, Array&gt;</returns>
		public static IDictionary<string, Array> SeparateArrayByProperties(this ICollection anonymousTypeSourceRows)
		{
			if (anonymousTypeSourceRows == null)
				throw new ArgumentNullException("sourceRows");

			int i = 0, size = anonymousTypeSourceRows.Count;
			Dictionary<string, Array> propArrayDict = new Dictionary<string, Array>();
			PropertyDescriptorCollection properties = null;

			foreach (var rowObj in anonymousTypeSourceRows)
			{
				if (properties == null)
				{
					properties = TypeDescriptor.GetProperties(rowObj);

					foreach (PropertyDescriptor prop in properties)
						propArrayDict.Add(prop.Name, Array.CreateInstance(prop.PropertyType, size));
				}

				foreach (PropertyDescriptor prop in properties)
					propArrayDict[prop.Name].SetValue(prop.GetValue(rowObj), i);

				i++;
			}

			return propArrayDict;
		}

		#endregion
	}
}
