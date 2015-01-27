using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Collections.Generic;
using DbParallel.DataAccess;

namespace DataBooster.DbWebApi
{
	public static class WebApiExtensions
	{
		const string CsvMediaTypeString = "text/csv";
		private static readonly MediaTypeHeaderValue CsvMediaType;

		static WebApiExtensions()
		{
			CsvMediaType = new MediaTypeHeaderValue(CsvMediaTypeString) { CharSet = "utf-8" };
		}

		public static void AddStreamMediaType(this HttpConfiguration config, string mediaType = CsvMediaTypeString)
		{
			var supported = config.Formatters[0].SupportedMediaTypes;
			supported.Add(new MediaTypeHeaderValue(mediaType));
		}

		internal static MediaTypeHeaderValue NegotiateMediaType(this HttpRequestMessage request)
		{
			HttpConfiguration configuration = request.GetConfiguration();
			IContentNegotiator contentNegotiator = configuration.Services.GetContentNegotiator();
			IEnumerable<MediaTypeFormatter> formatters = configuration.Formatters;
			ContentNegotiationResult result = contentNegotiator.Negotiate(typeof(StoredProcedureResponse), request, formatters);

			return (result == null) ? null : result.MediaType;
		}

		public static HttpResponseMessage ExecuteDbApi(this ApiController apiController, string sp, IDictionary<string, object> parameters)
		{
			var mediaType = apiController.Request.NegotiateMediaType();

			if (mediaType != null && mediaType.MediaType == CsvMediaTypeString)
			{
				HttpResponseMessage csvResponse = apiController.Request.CreateResponse();

				csvResponse.Content = new PushStreamContent((stream, httpContent, transportContext) =>
					{
						using (DbContext dbContext = new DbContext())
						{
							dbContext.ExecuteDbApi_CSV(sp, parameters, stream);
						}

						stream.Close();
					}, CsvMediaType);

				csvResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "[save_as].csv" };

				return csvResponse;
			}
			else
			{
				try
				{
					using (DbContext dbContext = new DbContext())
					{
						return apiController.Request.CreateResponse(HttpStatusCode.OK,
							dbContext.ExecuteDbApi(sp, parameters));
					}
				}
				catch (Exception e)
				{
					return apiController.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
				}
			}
		}
	}
}
