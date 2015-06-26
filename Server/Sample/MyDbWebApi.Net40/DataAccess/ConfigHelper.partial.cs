using System.Configuration;

namespace MyDbWebApi.DataAccess
{
	public static partial class ConfigHelper
	{
		private const string _UserNameReservedParameterSettingKey = "UserNameReservedParameter";

		private static string _UserNameReservedParameter = null;
		public static string UserNameReservedParameter
		{
			get
			{
				if (_UserNameReservedParameter == null)
				{
					_UserNameReservedParameter = ConfigurationManager.AppSettings[_UserNameReservedParameterSettingKey];

					if (_UserNameReservedParameter == null)
						_UserNameReservedParameter = string.Empty;
				}

				return _UserNameReservedParameter;
			}
			set
			{
				_UserNameReservedParameter = value;
			}
		}
	}
}