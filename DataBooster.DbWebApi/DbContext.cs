// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.Data.Common;
using System.Collections.Generic;
using DbParallel.DataAccess;
using DataBooster.DbWebApi.DataAccess;

namespace DataBooster.DbWebApi
{
	public class DbContext : IDisposable
	{
		private DbAccess _DbAccess;

		public DbContext(DbProviderFactory dbProviderFactory, string connectionString)
		{
			_DbAccess = new DbAccess(dbProviderFactory, connectionString);
			_DbAccess.DynamicPropertyNamingConvention = DbWebApiOptions.DefaultPropertyNamingConvention;
		}

		public DbContext()
			: this(ConfigHelper.DbProviderFactory, ConfigHelper.ConnectionString)
		{
		}

		public void SetNamingConvention(Dictionary<string, string> queryStrings)
		{
			string queryNamingCase = queryStrings.GetQueryParameterValue(DbWebApiOptions.QueryStringContract.NamingCaseParameterName);

			if (!string.IsNullOrEmpty(queryNamingCase))
				switch (char.ToUpper(queryNamingCase[0]))
				{
					case 'N':
						_DbAccess.DynamicPropertyNamingConvention = PropertyNamingConvention.None;
						break;
					case 'P':
						_DbAccess.DynamicPropertyNamingConvention = PropertyNamingConvention.PascalCase;
						break;
					case 'C':
						_DbAccess.DynamicPropertyNamingConvention = PropertyNamingConvention.CamelCase;
						break;
				}
		}

		public string ResolvePropertyName(string columnName)
		{
			return _DbAccess.DynamicPropertyNamingResolver(columnName);
		}

		public StoredProcedureResponse ExecuteDbApi(string sp, IDictionary<string, object> parameters)
		{
			return _DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters));
		}

		public object ExecuteDbApi(string sp, IDictionary<string, object> parameters, Action<int> exportResultSetStartTag, Action<DbDataReader> exportHeader, Action<DbDataReader> exportRow, Action<int> exportResultSetEndTag, IDictionary<string, object> outputParametersContainer, int[] resultSetChoices = null, bool bulkRead = false)
		{
			return _DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters), exportResultSetStartTag, exportHeader, exportRow, exportResultSetEndTag, outputParametersContainer, resultSetChoices, bulkRead);
		}

		// Invalidate Altered Stored Procedures from DerivedParametersCache
		internal int InvalidateAlteredSpFromCache(string spDetectDdlChanges, TimeSpan elapsedTime)
		{
			string commaDelimitedString = string.Join(",", _DbAccess.ListCachedStoredProcedures().OrderBy(sp => sp));

			if (string.IsNullOrEmpty(commaDelimitedString))
				return 0;

			var parameters = new Dictionary<string, IConvertible>(StringComparer.OrdinalIgnoreCase);
			parameters.Add(DbWebApiOptions.DetectDdlChangesContract.CommaDelimitedSpListParameterName, commaDelimitedString);
			parameters.Add(DbWebApiOptions.DetectDdlChangesContract.ElapsedTimeParameterName, (int)elapsedTime.TotalMinutes);

			StoredProcedureResponse results = _DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(spDetectDdlChanges, parameters));

			if (results.ResultSets.Count == 0 || results.ResultSets[0].Count == 0)
				return 0;
			else
				return _DbAccess.RemoveCachedStoredProcedures(results.ResultSets[0].Select(item => item.First().Value as string));
		}

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _DbAccess != null)
			{
				_DbAccess.Dispose();
				_DbAccess = null;
			}
		}
		#endregion
	}
}
