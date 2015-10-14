// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DbParallel.DataAccess;
using DataBooster.DbWebApi.Razor;
using DataBooster.DbWebApi.DataAccess;

namespace DataBooster.DbWebApi
{
	public static partial class WebApiExtensions
	{
		private static Collection<IFormatPlug> _FormatPlugs;
		private static PseudoContentNegotiator _PseudoContentNegotiator;
		private static CacheDictionary<Uri, IDictionary<string, string>> _QueryStringCache;
		private static TimeSpan _QueryStringCacheLifetime;

		/// <summary>
		/// Get/Set the duration time that query string dictionary remains in the cache, defaults to 3 minutes.
		/// </summary>
		public static TimeSpan QueryStringCacheLifetime
		{
			get { return _QueryStringCacheLifetime; }
			set { _QueryStringCacheLifetime = value; }
		}

		static WebApiExtensions()
		{
			_FormatPlugs = new Collection<IFormatPlug>();
			_PseudoContentNegotiator = new PseudoContentNegotiator();
			_QueryStringCache = new CacheDictionary<Uri, IDictionary<string, string>>();
			_QueryStringCacheLifetime = TimeSpan.FromSeconds(180);
		}

		#region Negotiation
		internal static ContentNegotiationResult Negotiate(this HttpRequestMessage request)
		{
			HttpConfiguration configuration = request.GetConfiguration();
			IContentNegotiator contentNegotiator = configuration.Services.GetContentNegotiator();
			IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;

			return contentNegotiator.Negotiate(typeof(StoredProcedureResponse), request, formatters);
		}

		internal static Encoding NegotiateEncoding(this HttpRequestMessage request, MediaTypeFormatter negotiatedFormatter)
		{
			return (negotiatedFormatter == null) ? null : _PseudoContentNegotiator.NegotiateEncoding(request, negotiatedFormatter);
		}

		private static IFormatPlug MatchFormatPlug(MediaTypeHeaderValue mediaType)
		{
			if (mediaType == null)
				return null;

			foreach (IFormatPlug formatPlug in _FormatPlugs)
				if (formatPlug.DefaultMediaType.Equals(mediaType) || formatPlug.SupportedMediaTypes.Contains(mediaType))
					return formatPlug;

			foreach (IFormatPlug formatPlug in _FormatPlugs)
				if (formatPlug.DefaultMediaType.MediaType == mediaType.MediaType || formatPlug.SupportedMediaTypes.Any(m => m.MediaType == mediaType.MediaType))
					return formatPlug;

			return null;
		}
		#endregion

		/// <summary>
		/// <para>ExecuteDbApi is the DbWebApi extension method to ApiController.</para>
		/// <para>See example at https://github.com/DataBooster/DbWebApi/blob/master/Examples/MyDbWebApi/Controllers/DbWebApiController.cs </para>
		/// </summary>
		/// <param name="apiController">Your ApiController to invoke this extension method</param>
		/// <param name="sp">Specifies the fully qualified name of database stored procedure or function</param>
		/// <param name="parameters">Specifies required parameters as name-value pairs</param>
		/// <returns>A complete HttpResponseMessage contains result data returned by the database</returns>
		public static HttpResponseMessage ExecuteDbApi(this ApiController apiController, string sp, IDictionary<string, object> parameters)
		{
			try
			{
				var negotiationResult = apiController.Request.Negotiate();

				if (negotiationResult != null && negotiationResult.Formatter is PseudoMediaTypeFormatter)
				{
					Encoding negotiatedEncoding = apiController.Request.NegotiateEncoding((negotiationResult == null) ? null : negotiationResult.Formatter);
					IFormatPlug formatPlug = MatchFormatPlug(negotiationResult.MediaType);

					if (formatPlug != null)
						return formatPlug.Respond(apiController, sp, parameters, negotiationResult.MediaType, negotiatedEncoding);
				}

				using (DalCenter dbContext = new DalCenter(apiController.Request.GetQueryStringDictionary()))
				{
					if (negotiationResult != null)
						if (negotiationResult.Formatter is RazorMediaTypeFormatter)
							return apiController.Request.CreateResponse(HttpStatusCode.OK, new RazorContext(dbContext.ExecuteDbApi(sp, parameters), parameters));

					return apiController.Request.CreateResponse(HttpStatusCode.OK, dbContext.ExecuteDbApi(sp, parameters));
				}
			}
			finally
			{
				CleanupCache(apiController.Request.RequestUri);
			}
		}

		/// <summary>
		/// Bulk execute a DbApi with a IList&lt;IDictionary&lt;string, object&gt;&gt; (a collection of input parameters collection)
		/// </summary>
		/// <typeparam name="T">IDictionary&lt;string, object&gt;</typeparam>
		/// <param name="apiController">Your ApiController to invoke this extension method</param>
		/// <param name="sp">Specifies the fully qualified name of database stored procedure or function</param>
		/// <param name="listOfParameters">Specifies a collection of required parameter dictionary for every call in the bulk execution</param>
		/// <returns>A complete HttpResponseMessage contains an array of every result data returned by the database</returns>
		public static HttpResponseMessage BulkExecuteDbApi<T>(this ApiController apiController, string sp, IList<T> listOfParameters) where T : IDictionary<string, object>
		{
			if (listOfParameters == null || listOfParameters.Count == 0)
				return apiController.Request.CreateResponse(HttpStatusCode.BadRequest);

			try
			{
				var negotiationResult = apiController.Request.Negotiate();

				if (negotiationResult != null)
					if (negotiationResult.Formatter is PseudoMediaTypeFormatter || negotiationResult.Formatter is RazorMediaTypeFormatter)
						return apiController.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);

				using (DalCenter dbContext = new DalCenter(apiController.Request.GetQueryStringDictionary()))
				{
					StoredProcedureResponse[] spResponses = new StoredProcedureResponse[listOfParameters.Count];

					for (int i = 0; i < spResponses.Length; i++)
						spResponses[i] = dbContext.ExecuteDbApi(sp, listOfParameters[i]);

					return apiController.Request.CreateResponse(HttpStatusCode.OK, spResponses.AsQueryable());
				}
			}
			finally
			{
				CleanupCache(apiController.Request.RequestUri);
			}
		}

		private static void CleanupCache(Uri requestUri)
		{
			_QueryStringCache.TryRemove(requestUri);
			_QueryStringCache.RemoveExpiredKeys(_QueryStringCacheLifetime);
			SelfRecoverDerivedParametersCache();
		}
	}
}
