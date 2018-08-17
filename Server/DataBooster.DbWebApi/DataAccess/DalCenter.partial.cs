// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.Data.Common;
using System.Collections.Generic;
using DbParallel.DataAccess;

namespace DataBooster.DbWebApi.DataAccess
{
	public partial class DalCenter
	{
		public DalCenter(IDictionary<string, object> queryStringsForDynamicDataStyle)
			: this()
		{
			SetDynamicDataStyle(queryStringsForDynamicDataStyle);
		}

		public void SetDynamicDataStyle(IDictionary<string, object> queryStrings)
		{
			string queryNamingCase = queryStrings.GetQueryParameterValue(DbWebApiOptions.QueryStringContract.NamingCaseParameterName);

			if (string.IsNullOrEmpty(queryNamingCase))
				DynamicPropertyNamingConvention = DbWebApiOptions.DefaultPropertyNamingConvention;
			else
				switch (char.ToUpper(queryNamingCase[0]))
				{
					case 'N':
						DynamicPropertyNamingConvention = PropertyNamingConvention.None;
						break;
					case 'P':
						DynamicPropertyNamingConvention = PropertyNamingConvention.PascalCase;
						break;
					case 'C':
						DynamicPropertyNamingConvention = PropertyNamingConvention.CamelCase;
						break;
					default:
						DynamicPropertyNamingConvention = DbWebApiOptions.DefaultPropertyNamingConvention;
						break;
				}

			string xmlAsAttribute = queryStrings.GetQueryParameterValue(DbWebApiOptions.QueryStringContract.XmlAsAttributeParameterName);
			if (!string.IsNullOrEmpty(xmlAsAttribute))
			{
				bool serializePropertyAsAttribute;

				if (bool.TryParse(xmlAsAttribute, out serializePropertyAsAttribute))
					AccessChannel.DynamicObjectXmlSettings.SerializePropertyAsAttribute = serializePropertyAsAttribute;
			}

			string xmlNullValue = queryStrings.GetQueryParameterValue(DbWebApiOptions.QueryStringContract.XmlNullValueParameterName);
			if (!string.IsNullOrEmpty(xmlNullValue))
			{
				bool emitNullValue;

				if (bool.TryParse(xmlNullValue, out emitNullValue))
					AccessChannel.DynamicObjectXmlSettings.EmitNullValue = emitNullValue;
			}

			string xmlTypeSchema = queryStrings.GetQueryParameterValue(DbWebApiOptions.QueryStringContract.XmlTypeSchemaParameterName);
			if (!string.IsNullOrEmpty(xmlTypeSchema))
			{
				BindableDynamicObject.XmlSettings.DataTypeSchema dataTypeSchema;

				if (Enum.TryParse(xmlTypeSchema, true, out dataTypeSchema))
					AccessChannel.DynamicObjectXmlSettings.TypeSchema = dataTypeSchema;
			}
		}

		//public string ResolvePropertyName(string columnName)
		//{
		//	return AccessChannel.DynamicPropertyNamingResolver(columnName);
		//}

		public StoredProcedureResponse ExecuteDbApi(string sp, IDictionary<string, object> parameters)
		{
			return base.ExecuteProcedure(sp, parameters.PretreatInputDictionary());
		}

		public object ExecuteDbApi(string sp, IDictionary<string, object> parameters, Action<int> exportResultSetStartTag, Action<DbDataReader> exportHeader, Action<DbDataReader> exportRow, Action<int> exportResultSetEndTag, IDictionary<string, object> outputParametersContainer, int[] resultSetChoices = null, bool bulkRead = false)
		{
			return AccessChannel.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters.PretreatInputDictionary()), exportResultSetStartTag, exportHeader, exportRow, exportResultSetEndTag, outputParametersContainer, resultSetChoices, bulkRead);
		}

		// Invalidate Altered Stored Procedures from DerivedParametersCache
		internal int InvalidateAlteredSpFromCache(string spDetectDdlChanges, TimeSpan elapsedTime)
		{
			string commaDelimitedString = string.Join(",", AccessChannel.ListCachedStoredProcedures().OrderBy(sp => sp));

			if (string.IsNullOrEmpty(commaDelimitedString))
				return 0;

			var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			parameters.Add(DbWebApiOptions.DetectDdlChangesContract.CommaDelimitedSpListParameterName, commaDelimitedString);
			parameters.Add(DbWebApiOptions.DetectDdlChangesContract.ElapsedTimeParameterName, (int)elapsedTime.TotalMinutes);

			StoredProcedureResponse results = AccessChannel.ExecuteStoredProcedure(new StoredProcedureRequest(spDetectDdlChanges, parameters));

			if (results.ResultSets.Count == 0 || results.ResultSets[0].Count == 0)
				return 0;
			else
				return AccessChannel.RemoveCachedStoredProcedures(results.ResultSets[0].Select(item => item.First().Value as string));
		}
	}
}
