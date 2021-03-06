﻿// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System.IO;
using System.Text;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using DbParallel.DataAccess;
using DataBooster.DbWebApi.DataAccess;

namespace DataBooster.DbWebApi.Csv
{
	public class CsvFormatPlug : IFormatPlug
	{
		private readonly MediaTypeHeaderValue[] _SupportedMediaTypes;

		public CsvFormatPlug()
		{
			_SupportedMediaTypes = new MediaTypeHeaderValue[] { DefaultMediaType };
		}

		public MediaTypeHeaderValue DefaultMediaType
		{
			get { return MediaTypeConstants.TextCsvMediaType; }
		}

		public IEnumerable<MediaTypeHeaderValue> SupportedMediaTypes
		{
			get { return _SupportedMediaTypes; }
		}

		public string FormatShortName
		{
			get { return "csv"; }
		}

		private int GetQueryResultSetIndex(IDictionary<string, object> queryStrings, string queryName)
		{
			string queryResultSet = queryStrings.GetQueryParameterValue(queryName);

			if (!string.IsNullOrEmpty(queryResultSet))
			{
				int resultSetIndex;

				if (int.TryParse(queryResultSet, out resultSetIndex))
					if (resultSetIndex > 0 && resultSetIndex < 1024)
						return resultSetIndex;
			}

			return 0;
		}

		public HttpResponseMessage Respond(ApiController apiController, string sp, IDictionary<string, object> parameters,
			MediaTypeHeaderValue negotiatedMediaType, Encoding negotiatedEncoding)
		{
			HttpResponseMessage csvResponse = apiController.Request.CreateResponse();
			IDictionary<string, object> queryStrings = apiController.Request.GetQueryStringDictionary();
			int[] resultSetChoices = new int[] { GetQueryResultSetIndex(queryStrings, DbWebApiOptions.QueryStringContract.ResultSetParameterName) };

			csvResponse.Content = new PushStreamContent((stream, httpContent, transportContext) =>
			{
				StreamWriter textWriter = (negotiatedEncoding == null) ? new StreamWriter(stream) : new StreamWriter(stream, negotiatedEncoding);

				using (DalCenter dbContext = new DalCenter(queryStrings))
				{
					CsvExporter csvExporter = new CsvExporter(textWriter);

					dbContext.ExecuteDbApi(sp, parameters, null,
						readHeader =>
						{
							csvExporter.WriteHeader(dbContext.NameAllFields(readHeader));
						},
						readRow =>
						{
							object[] values = new object[csvExporter.ColumnCount];

							readRow.GetColumnValues(values);
							csvExporter.WriteRow(values);
						},
						null, null, resultSetChoices);

					textWriter.Flush();
				}

				stream.Close();
			}, negotiatedMediaType ?? DefaultMediaType);

			csvResponse.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = queryStrings.GetQueryFileName(DbWebApiOptions.QueryStringContract.FileNameParameterName, FormatShortName) };

			return csvResponse;
		}
	}
}
