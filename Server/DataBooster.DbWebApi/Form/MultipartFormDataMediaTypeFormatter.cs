// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace DataBooster.DbWebApi.Form
{
	/// <summary>
	/// <see cref="MediaTypeFormatter"/> class for handling HTML Form Data - File Upload and Multipart MIME, also known as <c>multipart/form-data</c>. 
	/// </summary>
	public class MultipartFormDataMediaTypeFormatter : MediaTypeFormatter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MultipartFormDataMediaTypeFormatter"/> class.
		/// </summary>
		public MultipartFormDataMediaTypeFormatter()
		{
			SupportedMediaTypes.Add(DefaultMediaType);
		}

		public static MediaTypeHeaderValue DefaultMediaType
		{
			get { return MediaTypeConstants.MultipartFormDataMediaType; }
		}

		public override bool CanReadType(Type type)
		{
			return type == typeof(InputParameters);
		}

		public override bool CanWriteType(Type type)
		{
			return false;
		}

		/// <param name="type">The type of the object to deserialize.</param>
		/// <param name="readStream">The <see cref="T:System.IO.Stream"/> to read.</param>
		/// <param name="content">The <see cref="T:System.Net.Http.HttpContent"/>, if available. It may be null.</param>
		/// <param name="formatterLogger">The <see cref="T:System.Net.Http.Formatting.IFormatterLogger"/> to log events to.</param>
		/// <returns>A <see cref="T:System.Threading.Tasks.Task"/> whose result will be an object of the given type.</returns>
		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!CanReadType(type))
				throw new InvalidOperationException();

			if (content == null)
				throw new ArgumentNullException("content");

			if (!content.IsMimeMultipartContent())
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

			try
			{
				return ReadFormDataAsync(content);
			}
			catch (Exception e)
			{
				if (formatterLogger == null)
					throw;

				formatterLogger.LogError(string.Empty, e);
#if WEB_API2
				return Task.FromResult<object>(new InputParameters());
#else  // ASP.NET Web API 1
				var tcs = new TaskCompletionSource<object>();
				tcs.SetResult(new InputParameters());
				return tcs.Task;
#endif
			}
		}

#if WEB_API2
		private async Task<object> ReadFormDataAsync(HttpContent content)
		{
			var multipartProvider = new MultipartFormDataMemoryStreamProvider();

			await content.ReadAsMultipartAsync(multipartProvider).ConfigureAwait(false);

			return new InputParameters(multipartProvider.GetAllInputData());
		}
#else  // ASP.NET Web API 1
		private Task<object> ReadFormDataAsync(HttpContent content)
		{
			var multipartProvider = new MultipartFormDataMemoryStreamProvider();

			return content.ReadAsMultipartAsync(multipartProvider).ContinueWith<object>(
				pTask =>
				{
					if (pTask.IsFaulted)
						throw pTask.Exception;

					if (pTask.IsCanceled)
						if (pTask.Exception == null)
							throw new TaskCanceledException();
						else
							throw pTask.Exception;

					return new InputParameters(pTask.Result.GetAllInputData());
				});
		}
#endif
	}
}
