// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace DataBooster.DbWebApi.Jsonp
{
	public class JsonpMediaTypeFormatter : JsonMediaTypeFormatter
	{
		const string _FormatShortName = "jsonp";
		private static readonly MediaTypeHeaderValue _textJavaScript = new MediaTypeHeaderValue("text/javascript");
		private static readonly MediaTypeHeaderValue _applicationJavaScript = new MediaTypeHeaderValue("application/javascript");
		private static readonly MediaTypeHeaderValue _applicationJsonp = new MediaTypeHeaderValue("application/json-p");
		private readonly string _CallbackQueryParameter;
		private string _Callback;

		public JsonpMediaTypeFormatter(string callbackQueryParameter = null)
		{
			_CallbackQueryParameter = string.IsNullOrEmpty(callbackQueryParameter) ? DbWebApiOptions.QueryStringContract.JsonpCallbackParameterName : callbackQueryParameter;

			SupportedMediaTypes.Add(_textJavaScript);
			SupportedMediaTypes.Add(_applicationJavaScript);
			SupportedMediaTypes.Add(_applicationJsonp);

			MediaTypeMappings.Add(new JsonpQueryStringMapping(_CallbackQueryParameter, _textJavaScript));
			MediaTypeMappings.Add(new QueryStringMapping(DbWebApiOptions.QueryStringContract.MediaTypeParameterName, _FormatShortName, _textJavaScript));
			MediaTypeMappings.Add(new UriPathExtensionMapping(_FormatShortName, _textJavaScript));
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

			_Callback = JsonpQueryStringMapping.GetCallback(request, _CallbackQueryParameter);

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


		}
	}
}
