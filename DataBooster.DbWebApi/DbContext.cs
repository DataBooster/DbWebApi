// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
			if (queryStrings != null && queryStrings.Count > 0)
			{
				string queryNamingCase;

				if (queryStrings.TryGetValue(DbWebApiOptions.QueryStringContract.NamingCaseParameterName, out queryNamingCase))
					SetDynamicPropertyNamingConvention(queryNamingCase);
			}
		}

		private void SetDynamicPropertyNamingConvention(string queryNamingCase)
		{
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
