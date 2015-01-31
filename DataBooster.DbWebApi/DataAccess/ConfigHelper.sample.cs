using System;
using System.Data.Common;
using System.Configuration;

namespace DataBooster.DbWebApi.DataAccess
{
	public static partial class ConfigHelper
	{
		#region Setting key names defined in your config file, can be overridden in partial OnInitializing()
		private static string _ConnectionSettingKey = "DataBooster.DbWebApi.MainConnection";
		private static string _PackageSettingKey = "DataBooster.DbWebApi.MainPackage";
		#endregion

		#region Properties
		private static DbProviderFactory _DbProviderFactory;
		public static DbProviderFactory DbProviderFactory
		{
			get
			{
				return _DbProviderFactory;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("DbProviderFactory");
				_DbProviderFactory = value;
			}
		}

		private static string _ConnectionString;
		public static string ConnectionString
		{
			get
			{
				return _ConnectionString;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("ConnectionString");
				_ConnectionString = value;
			}
		}

		private static string _DatabasePackage;
		public static string DatabasePackage
		{
			get { return _DatabasePackage; }
			set { _DatabasePackage = value ?? string.Empty; }
		}
		#endregion

		static ConfigHelper()
		{
			OnInitializing();

			#region Default Initialization
			ConnectionStringSettings connSetting = ConfigurationManager.ConnectionStrings[_ConnectionSettingKey];
			_DbProviderFactory = DbProviderFactories.GetFactory(connSetting.ProviderName);
			_ConnectionString = connSetting.ConnectionString;

			_DatabasePackage = ConfigurationManager.AppSettings[_PackageSettingKey];
			if (_DatabasePackage == null)
				_DatabasePackage = string.Empty;
			#endregion

			OnInitialized();
		}

		static partial void OnInitializing();
		static partial void OnInitialized();
	}
}
