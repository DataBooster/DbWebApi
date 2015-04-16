using System;
using System.Data.Common;
using System.Configuration;

namespace DataBooster.DbWebApi.DataAccess
{
	public static partial class ConfigHelper
	{
		public static string ConnectionSettingKey
		{
			get
			{
				return _ConnectionSettingKey;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("ConnectionSettingKey");

				_ConnectionSettingKey = value;

				ConnectionStringSettings connSetting = ConfigurationManager.ConnectionStrings[_ConnectionSettingKey];
				_DbProviderFactory = DbProviderFactories.GetFactory(connSetting.ProviderName);
				_ConnectionString = connSetting.ConnectionString;
			}
		}
	}
}
