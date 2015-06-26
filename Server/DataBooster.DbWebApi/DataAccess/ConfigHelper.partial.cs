// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Data.Common;
using System.Configuration;

namespace DataBooster.DbWebApi.DataAccess
{
	public static partial class ConfigHelper
	{
		const string _DefaultDetectDdlChangesProcSettingKey = "DataBooster.DbWebApi.DetectDdlChangesProc";

		private static string _DetectDdlChangesProcSettingKey = _DefaultDetectDdlChangesProcSettingKey;
		public static string DetectDdlChangesProcSettingKey
		{
			get
			{
				return _DetectDdlChangesProcSettingKey;
			}
			set
			{
				string newSettingKey = string.IsNullOrEmpty(value) ? _DefaultDetectDdlChangesProcSettingKey : value;

				if (newSettingKey != _DetectDdlChangesProcSettingKey)
				{
					_DetectDdlChangesProcSettingKey = newSettingKey;
					_DetectDdlChangesProc = ReadDetectDdlChangesProcSetting();
				}
			}
		}

		private static string ReadDetectDdlChangesProcSetting()
		{
			return ConfigurationManager.AppSettings[_DetectDdlChangesProcSettingKey] ?? string.Empty;
		}

		private static string _DetectDdlChangesProc;
		public static string DetectDdlChangesProc
		{
			get
			{
				if (_DetectDdlChangesProc == null)
					_DetectDdlChangesProc = ReadDetectDdlChangesProcSetting();

				return _DetectDdlChangesProc;
			}
			set
			{
				_DetectDdlChangesProc = value;
			}
		}


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
