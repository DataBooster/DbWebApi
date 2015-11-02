// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System.IO;
using System.Text;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using ClosedXML.Excel;
using DataBooster.DbWebApi.DataAccess;

namespace DataBooster.DbWebApi.Excel
{
	public class XlsxFormatPlug : IFormatPlug
	{
		private readonly MediaTypeHeaderValue[] _SupportedMediaTypes;

		public XlsxFormatPlug()
		{
			_SupportedMediaTypes = new MediaTypeHeaderValue[] { MediaTypeConstants.ApplicationVndOpenXmlSheetMediaType,
				MediaTypeConstants.ApplicationMsExcelMediaType, MediaTypeConstants.ApplicationXlsxMediaType };
		}

		public MediaTypeHeaderValue DefaultMediaType
		{
			get { return MediaTypeConstants.ApplicationVndOpenXmlSheetMediaType; }
		}

		public IEnumerable<MediaTypeHeaderValue> SupportedMediaTypes
		{
			get { return _SupportedMediaTypes; }
		}

		public string FormatShortName
		{
			get { return "xlsx"; }
		}

		public HttpResponseMessage Respond(ApiController apiController, string sp, IDictionary<string, object> parameters,
			MediaTypeHeaderValue negotiatedMediaType, Encoding negotiatedEncoding)
		{
			HttpResponseMessage csvResponse = apiController.Request.CreateResponse();
			IDictionary<string, object> queryStrings = apiController.Request.GetQueryStringDictionary();
			MemoryStream memoryStream = new MemoryStream();	// TBD: To find a more efficient way later

			using (XLWorkbook workbook = new XLWorkbook())
			using (DalCenter dbContext = new DalCenter(queryStrings))
			{
				IXLWorksheet currentWorksheet = null;

				dbContext.ExecuteDbApi(sp, parameters, rs =>
					{
						currentWorksheet = workbook.AddWorksheet(string.Format("Sheet{0}", rs + 1));
					},
					header =>
					{
						if (currentWorksheet != null)
							for (int col = 0; col < header.VisibleFieldCount; col++)
								currentWorksheet.Cell(1, col + 1).SetValue(dbContext.ResolvePropertyName(header.GetName(col))).Style.Font.Bold = true;
					},
					rows =>
					{
						if (currentWorksheet != null)
							currentWorksheet.Cell(2, 1).Value = rows;
					},
					foot =>
					{
						if (currentWorksheet != null)
							currentWorksheet.Columns().AdjustToContents();
					}, null, null, true);

				workbook.SaveAs(memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
			}

			csvResponse.Content = new StreamContent(memoryStream);
			csvResponse.Content.Headers.ContentType = negotiatedMediaType;
			csvResponse.Content.Headers.ContentLength = memoryStream.Length;
			csvResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = queryStrings.GetQueryFileName(DbWebApiOptions.QueryStringContract.FileNameParameterName, FormatShortName) };

			return csvResponse;
		}
	}
}
