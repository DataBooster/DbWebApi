// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using DbParallel.DataAccess;

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

		internal static T ReadAs<T>(this HttpResponseMessage httpResponse) where T : class
		{
			var content = httpResponse.Content;

			if (content == null)
				throw new ArgumentNullException("httpResponse.Content");

			if (httpResponse.IsSuccessStatusCode)
			{
				Type type = typeof(T);

				if (typeof(string) == type)
					return content.ReadContentAsString() as T;

				if (typeof(Stream) == type)
					return content.ReadContentAsStream() as T;

				if (typeof(XDocument) == type)
					return content.ReadContentAsXDocument() as T;

				if (typeof(XElement) == type)
					return content.ReadContentAsXElement() as T;

				return content.ReadContentAs<T>();
			}
			else
				throw httpResponse.CreateUnsuccessException();
		}

		private static T ReadContentAs<T>(this HttpContent content)
		{
			Task<T> readTask = content.ReadAsAsync<T>(ReadAsMediaTypeFormatterCollection);
			T result = readTask.Result;

			if (readTask.IsFaulted)
				throw readTask.Exception;
			else
				return result;
		}

		private static string ReadContentAsString(this HttpContent content)
		{
			Task<string> readTask = content.ReadAsStringAsync();
			string result = readTask.Result;

			if (readTask.IsFaulted)
				throw readTask.Exception;
			else
				return result;
		}

		private static Stream ReadContentAsStream(this HttpContent content)
		{
			Task<Stream> readTask = content.ReadAsStreamAsync();
			Stream result = readTask.Result;

			if (readTask.IsFaulted)
				throw readTask.Exception;
			else
				return result;
		}

		private static XDocument ReadContentAsXDocument(this HttpContent content)
		{
			return XDocument.Load(content.ReadContentAsStream());
		}

		private static XElement ReadContentAsXElement(this HttpContent content)
		{
			return XElement.Load(content.ReadContentAsStream());
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

		private static MediaTypeHeaderValue GetContentType(this HttpContent content)
		{
			if (content == null)
				return null;
			if (content.Headers == null)
				return null;
			return content.Headers.ContentType;
		}

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
