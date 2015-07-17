using System.Configuration;

namespace MyDbWebApi
{
	public static class ConfigHelper
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