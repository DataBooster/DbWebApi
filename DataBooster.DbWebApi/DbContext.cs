using System;
using System.IO;
using System.Data.Common;
using System.Collections.Generic;
using DbParallel.DataAccess;
using DataBooster.DbWebApi.DataAccess;
using DataBooster.DbWebApi.Csv;

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

		public void ExecuteDbApi_CSV(string sp, IDictionary<string, object> parameters, TextWriter textWriter)
		{
			CsvExporter csvExporter = new CsvExporter(textWriter);

			_DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters), true, null,
				reader =>
				{
					string[] headers = new string[reader.VisibleFieldCount];

					for (int i = 0; i < headers.Length; i++)
						headers[i] = reader.GetName(i);

					csvExporter.WriteHeader(headers);
				},
				reader =>
				{
					object[] values = new object[reader.VisibleFieldCount];

					reader.GetValues(values);

					csvExporter.WriteRow(values);
				},
				null, null);

			textWriter.Flush();
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
