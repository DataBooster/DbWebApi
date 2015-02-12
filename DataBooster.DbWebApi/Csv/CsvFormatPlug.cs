// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace DataBooster.DbWebApi.Csv
{
	public class CsvFormatPlug : IFormatPlug
	{
		private readonly MediaTypeHeaderValue _DefaultMediaType;
		private readonly MediaTypeHeaderValue[] _SupportedMediaTypes;

		public CsvFormatPlug()
		{
			_DefaultMediaType = new MediaTypeHeaderValue("text/csv") { CharSet = "utf-8" };
			_SupportedMediaTypes = new MediaTypeHeaderValue[] { _DefaultMediaType };
		}

		public MediaTypeHeaderValue DefaultMediaType
		{
			get { return _DefaultMediaType; }
		}

		public IEnumerable<MediaTypeHeaderValue> SupportedMediaTypes
		{
			get { return _SupportedMediaTypes; }
		}

		public string FormatShortName
		{
			get { return "csv"; }
		}

		public HttpResponseMessage Respond(ApiController apiController, string sp, IDictionary<string, object> parameters,
			MediaTypeHeaderValue negotiatedMediaType, Encoding negotiatedEncoding)
		{
			HttpResponseMessage csvResponse = apiController.Request.CreateResponse();

			csvResponse.Content = new PushStreamContent((stream, httpContent, transportContext) =>
			{
				StreamWriter textWriter = (negotiatedEncoding == null) ? new StreamWriter(stream) : new StreamWriter(stream, negotiatedEncoding);

				using (DbContext dbContext = new DbContext())
				{
					CsvExporter csvExporter = new CsvExporter(textWriter);

					dbContext.ExecuteDbApi(sp, parameters, true, null,
						readHeader =>
						{
							string[] headers = new string[readHeader.VisibleFieldCount];

							for (int i = 0; i < headers.Length; i++)
								headers[i] = readHeader.GetName(i);

							csvExporter.WriteHeader(headers);
						},
						readRow =>
						{
							object[] values = new object[readRow.VisibleFieldCount];

							readRow.GetValues(values);

							csvExporter.WriteRow(values);
						},
						null, null);

					textWriter.Flush();
				}

				stream.Close();
			}, negotiatedMediaType ?? _DefaultMediaType);

			csvResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "[save_as]." + FormatShortName };

			return csvResponse;
		}
	}
}
