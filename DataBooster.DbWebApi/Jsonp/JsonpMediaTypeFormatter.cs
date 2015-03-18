// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataBooster.DbWebApi.Jsonp
{
	public class JsonpMediaTypeFormatter : JsonMediaTypeFormatter
	{
		const string _FormatShortName = "jsonp";
		private static readonly MediaTypeHeaderValue _TextJavaScript = new MediaTypeHeaderValue("text/javascript");
		private static readonly MediaTypeHeaderValue _ApplicationJavaScript = new MediaTypeHeaderValue("application/javascript");
		private static readonly MediaTypeHeaderValue _ApplicationJsonp = new MediaTypeHeaderValue("application/json-p");
		private readonly string _CallbackQueryParameter;
		private readonly string _JsonpStateQueryParameter;
		private string _Callback;
		private string _JsonpState;

		public JsonpMediaTypeFormatter(string callbackQueryParameter = null, string jsonpStateQueryParameter = null)
		{
			_CallbackQueryParameter = string.IsNullOrEmpty(callbackQueryParameter) ? DbWebApiOptions.QueryStringContract.JsonpCallbackParameterName : callbackQueryParameter;
			_JsonpStateQueryParameter = string.IsNullOrEmpty(jsonpStateQueryParameter) ? DbWebApiOptions.QueryStringContract.JsonpStateParameterName : jsonpStateQueryParameter;

			SupportedMediaTypes.Add(_TextJavaScript);
			SupportedMediaTypes.Add(_ApplicationJavaScript);
			SupportedMediaTypes.Add(_ApplicationJsonp);

			MediaTypeMappings.Add(new JsonpQueryStringMapping(_CallbackQueryParameter, _TextJavaScript));
			MediaTypeMappings.Add(new QueryStringMapping(DbWebApiOptions.QueryStringContract.MediaTypeParameterName, _FormatShortName, _TextJavaScript));
			MediaTypeMappings.Add(new UriPathExtensionMapping(_FormatShortName, _TextJavaScript));
		}

		/// <param name="type">The type to format.</param>
		/// <param name="request">The request.</param>
		/// <param name="mediaType">The media type.</param>
		/// <returns>Returns <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter"/>.</returns>
		public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (request == null)
				throw new ArgumentNullException("request");

			Dictionary<string, string> queryStrings = request.GetQueryStringDictionary();

			_Callback = queryStrings.GetQueryParameterValue(_CallbackQueryParameter);
			_JsonpState = queryStrings.GetQueryParameterValue(_JsonpStateQueryParameter);

			return this;
		}

		/// <param name="type">The type of object to write.</param>
		/// <param name="value">The object to write.</param>
		/// <param name="writeStream">The <see cref="T:System.IO.Stream"/> to which to write.</param>
		/// <param name="content">The <see cref="T:System.Net.Http.HttpContent"/> where the content is being written.</param>
		/// <param name="transportContext">The <see cref="T:System.Net.TransportContext"/>.</param>
		/// <returns>A <see cref="T:System.Threading.Tasks.Task"/> that will write the value to the stream.</returns>
		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (writeStream == null)
				throw new ArgumentNullException("writeStream");

			if (string.IsNullOrEmpty(_Callback))
				return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);

			var encoding = SelectCharacterEncoding(content == null ? null : content.Headers);
			var writer = new StreamWriter(writeStream, encoding);

			writer.Write(_Callback + "(");
			writer.Flush();

			return base.WriteToStreamAsync(type, value, writeStream, content, transportContext).ContinueWith(jsonTask =>
				{
					if (jsonTask.IsCanceled)
						return;
					if (jsonTask.IsFaulted)
						throw jsonTask.Exception;

					if (!string.IsNullOrEmpty(_JsonpState))
					{
						writer.Write(", ");
						writer.Write(_JsonpState);
					}

					writer.Write(");");
					writer.Flush();
				});
		}
	}
}
