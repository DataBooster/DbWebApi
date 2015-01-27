using System.Data.Common;
using System.Configuration;

namespace DataBooster.DbWebApi.DataAccess
{
	public static partial class ConfigHelper
	{
		#region Setting key names defined in your config file, can be overridden in partial OnInitializing()
		private static string _ConnectionSettingKey = "DataBooster.DbWebApi.MainConnection";
		private static string _PackageSettingKey = "DataBooster.DbWebApi.MainPackage";
		private static string _AuxConnectionSettingKey = "DataBooster.DbWebApi.AuxConnection";
		#endregion

		#region Properties
		private static DbProviderFactory _DbProviderFactory;
		public static DbProviderFactory DbProviderFactory
		{
			get { return _DbProviderFactory; }
		}

		private static string _ConnectionString;
		public static string ConnectionString
		{
			get { return _ConnectionString; }
		}

		private static string _DatabasePackage;
		public static string DatabasePackage
		{
			get { return _DatabasePackage; }
		}

		private static DbProviderFactory _AuxDbProviderFactory;
		public static DbProviderFactory AuxDbProviderFactory
		{
			get { return _AuxDbProviderFactory; }
		}

		private static string _AuxConnectionString;
		public static string AuxConnectionString
		{
			get { return _AuxConnectionString; }
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

			if (string.IsNullOrWhiteSpace(_AuxConnectionSettingKey) == false)
			{
				connSetting = ConfigurationManager.ConnectionStrings[_AuxConnectionSettingKey];

				if (connSetting != null)
				{
					_AuxDbProviderFactory = DbProviderFactories.GetFactory(connSetting.ProviderName);
					_AuxConnectionString = connSetting.ConnectionString;
				}
			}
			#endregion

			OnInitialized();
		}

		static partial void OnInitializing();
		static partial void OnInitialized();
	}
}
