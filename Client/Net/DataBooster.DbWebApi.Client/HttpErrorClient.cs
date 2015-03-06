// Code based on: https://github.com/ASP-NET-MVC/aspnetwebstack/blob/master/src/System.Web.Http/HttpError.cs

using System;
using System.Collections.Generic;

namespace DataBooster.DbWebApi.Client
{
	public class HttpErrorClient : Dictionary<string, object>
	{
		private const string MessageKey = "Message";
		private const string MessageDetailKey = "MessageDetail";
		private const string ModelStateKey = "ModelState";
		private const string ExceptionMessageKey = "ExceptionMessage";
		private const string ExceptionTypeKey = "ExceptionType";
		private const string StackTraceKey = "StackTrace";
		private const string InnerExceptionKey = "InnerException";

		public string Message
		{
			get { return GetPropertyValue<String>(MessageKey); }
			set { this[MessageKey] = value; }
		}

		public string MessageDetail
		{
			get { return GetPropertyValue<String>(MessageDetailKey); }
			set { this[MessageDetailKey] = value; }
		}

		public string ExceptionMessage
		{
			get { return GetPropertyValue<String>(ExceptionMessageKey); }
			set { this[ExceptionMessageKey] = value; }
		}

		public string ExceptionType
		{
			get { return GetPropertyValue<String>(ExceptionTypeKey); }
			set { this[ExceptionTypeKey] = value; }
		}

		public string StackTrace
		{
			get { return GetPropertyValue<String>(StackTraceKey); }
			set { this[StackTraceKey] = value; }
		}

		public HttpErrorClient ModelState
		{
			get { return GetPropertyValue<HttpErrorClient>(ModelStateKey); }
		}

		public HttpErrorClient InnerException
		{
			get { return GetPropertyValue<HttpErrorClient>(InnerExceptionKey); }
		}

		public TValue GetPropertyValue<TValue>(string key)
		{
			object value;

			if (this.TryGetValue(key, out value))
				return (TValue)value;

			return default(TValue);
		}
	}
}
