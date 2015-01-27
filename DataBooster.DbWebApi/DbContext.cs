using System;
using System.IO;
using System.Data.Common;
using System.Collections.Generic;
using DbParallel.DataAccess;
using DataBooster.DbWebApi.DataAccess;
using ServiceStack.Text;

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

		public void ExecuteDbApi_CSV(string sp, IDictionary<string, object> parameters, Stream stream)
		{
			StreamWriter writer = new StreamWriter(stream);
			CsvConfig<Dictionary<string, object>>.OmitHeaders = true;
			var row = new Dictionary<string, object>[1];

			_DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters), true, null,
				reader =>
				{
					string[] headers = new string[reader.VisibleFieldCount];

					for (int i = 0; i < headers.Length; i++)
						headers[i] = reader.GetName(i);

					CsvWriter<string>.WriteRow(writer, headers);
				},
				reader =>
				{
					row[0] = ReadRowAsDictionary(reader);
					CsvWriter<Dictionary<string, object>>.Write(writer, row);
				},
				null, null);
		}

		private Dictionary<string, object> ReadRowAsDictionary(DbDataReader reader)
		{
			Dictionary<string, object> row = new Dictionary<string, object>(reader.VisibleFieldCount);

			for (int c = 0; c < reader.VisibleFieldCount; c++)
				row.Add(c.ToString(), reader[c]);

			return row;
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
