using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Linq;
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

		public static void SupportCsvMediaType(this HttpConfiguration config)
		{
			MediaTypeFormatter mediaTypeFormatter = config.Formatters.JsonFormatter;

			mediaTypeFormatter.AddMediaTypeMapping("json", new MediaTypeHeaderValue("application/json"));

			if (config.Formatters.XmlFormatter != null)
			{
				mediaTypeFormatter = config.Formatters.XmlFormatter;
				mediaTypeFormatter.AddMediaTypeMapping("xml", new MediaTypeHeaderValue("application/xml"));
			}

			mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(CsvMediaTypeString));
			mediaTypeFormatter.AddMediaTypeMapping("csv", CsvMediaType);
		}

		private static void AddMediaTypeMapping(this MediaTypeFormatter mediaTypeFormatter, string type, MediaTypeHeaderValue mediaType)
		{
			if (mediaTypeFormatter != null && !mediaTypeFormatter.MediaTypeMappings.Any(m => m.ExistMediaTypeMapping(type)))
			{
				mediaTypeFormatter.AddQueryStringMapping("media", type, mediaType);
				mediaTypeFormatter.AddUriPathExtensionMapping(type, mediaType);
			}
		}

		private static bool ExistMediaTypeMapping(this MediaTypeMapping mediaTypeMapping, string media)
		{
			QueryStringMapping qsMapping = mediaTypeMapping as QueryStringMapping;

			if (qsMapping != null && qsMapping.QueryStringParameterValue == media)
				return true;

			UriPathExtensionMapping ueMapping = mediaTypeMapping as UriPathExtensionMapping;

			if (ueMapping != null && ueMapping.UriPathExtension == media)
				return true;

			return false;
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
					StreamWriter textWriter = new StreamWriter(stream, new UTF8Encoding());

					using (DbContext dbContext = new DbContext())
					{
						dbContext.ExecuteDbApi_CSV(sp, parameters, textWriter);
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
