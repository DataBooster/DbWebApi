// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Text;
using System.Net.Http;
using System.Collections;

namespace DataBooster.DbWebApi.Client
{
	public class HttpResponseClientException : HttpRequestException
	{
		private HttpErrorClient _ErrorDictionary;
		private string _DetailMessage;

		public HttpResponseClientException(HttpErrorClient errorFromServer)
		{
			if (errorFromServer == null)
				throw new ArgumentNullException("errorFromServer");

			_ErrorDictionary = errorFromServer;
			_DetailMessage = BuildMessage(_ErrorDictionary);
		}

		private string BuildMessage(HttpErrorClient errorDictionary)
		{
			StringBuilder detailMessage = new StringBuilder(errorDictionary.Message);

			if (!string.IsNullOrEmpty(errorDictionary.ExceptionMessage))
				detailMessage.AppendFormat(" ({0})", errorDictionary.ExceptionMessage);

			if (!string.IsNullOrEmpty(errorDictionary.MessageDetail))
			{
				detailMessage.AppendLine();
				detailMessage.Append(errorDictionary.MessageDetail);
			}

			return detailMessage.ToString();
		}

		public override IDictionary Data
		{
			get { return _ErrorDictionary; }
		}

		public override string Message
		{
			get { return _DetailMessage; }
		}

		public override string Source
		{
			get { return _ErrorDictionary.ExceptionType; }
		}

		public override string StackTrace
		{
			get { return _ErrorDictionary.StackTrace; }
		}
	}
}
