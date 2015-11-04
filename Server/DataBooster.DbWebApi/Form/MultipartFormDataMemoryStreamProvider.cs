// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataBooster.DbWebApi.Form
{
	public class MultipartFormDataMemoryStreamProvider : MultipartFormDataStreamProvider	// Not base on MultipartFormDataRemoteStreamProvider for compatibility with Wep API 1
	{
		private Collection<KeyValuePair<HttpContentHeaders, MemoryStream>> _Streams;

		public MultipartFormDataMemoryStreamProvider()
			: base(".")
		{
			_Streams = new Collection<KeyValuePair<HttpContentHeaders, MemoryStream>>();
		}

		public IEnumerable<KeyValuePair<string, byte[]>> GetUploadedData()
		{
			return _Streams.Select(p => new KeyValuePair<string, byte[]>(GetFormFieldName(p.Key), p.Value.ToArray()));
		}

		/// <param name="parent">The HTTP content that contains this body part.</param>
		/// <param name="headers">Header fields describing the body part.</param>
		/// <returns>The <see cref="T:System.IO.Stream"/> instance where the message body part is written.</returns>
		public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			if (headers == null)
				throw new ArgumentNullException("headers");

			MemoryStream memoryStream = new MemoryStream();

			string localFileName = GetFileName(headers);
			if (localFileName != null)
			{
				FileData.Add(new MultipartFileData(headers, localFileName));
				_Streams.Add(new KeyValuePair<HttpContentHeaders, MemoryStream>(headers, memoryStream));
			}

			return memoryStream;
		}

		/// <param name="headers">The headers for the current MIME body part.</param>
		/// <returns>A relative filename with no path component.</returns>
		public override string GetLocalFileName(HttpContentHeaders headers)
		{
			if (headers == null)
				throw new ArgumentNullException("headers");

			return GetFileName(headers);
		}

		private static string GetFormFieldName(HttpContentHeaders headers)
		{
			ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
			if (contentDisposition == null)
				throw new ArgumentNullException("headers.ContentDisposition");

			return UnquoteToken(contentDisposition.Name);
		}

		private static string GetFileName(HttpContentHeaders headers)
		{
			ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
			if (contentDisposition == null)
				throw new ArgumentNullException("headers.ContentDisposition");

			return UnquoteToken(contentDisposition.FileName);
		}

		private static string UnquoteToken(string token)
		{
			if (string.IsNullOrEmpty(token))
				return null;

			if (token.Length >= 2 && token[0] == '"' && token[token.Length - 1] == '"')
				return token.Substring(1, token.Length - 2);
			else
				return token;
		}
	}
}
