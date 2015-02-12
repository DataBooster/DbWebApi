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
		}

		public DbContext()
			: this(ConfigHelper.DbProviderFactory, ConfigHelper.ConnectionString)
		{
		}

		public StoredProcedureResponse ExecuteDbApi(string sp, IDictionary<string, object> parameters)
		{
			return _DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters));
		}

		public object ExecuteDbApi(string sp, IDictionary<string, object> parameters, Func<int, bool> exportResultSetStartTag, Action<DbDataReader> exportHeader, Action<DbDataReader> exportRow, Action<int> exportResultSetEndTag, IDictionary<string, object> outputParametersContainer, bool exportOnlyOneResultSet = false, bool bulkRead = false)
		{
			return _DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters),
				exportResultSetStartTag, exportHeader, exportRow, exportResultSetEndTag,
				outputParametersContainer, exportOnlyOneResultSet, bulkRead);
		}

		public object ExecuteDbApi(string sp, IDictionary<string, object> parameters, bool exportFirstResultSetOnly, Action<int> exportResultSetStartTag, Action<DbDataReader> exportHeader, Action<DbDataReader> exportRow, Action<int> exportResultSetEndTag, IDictionary<string, object> outputParametersContainer, bool bulkRead = false)
		{
			return _DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters),
				exportFirstResultSetOnly, exportResultSetStartTag, exportHeader, exportRow, exportResultSetEndTag,
				outputParametersContainer, bulkRead);
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
