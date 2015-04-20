// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Web.Http;
using DbParallel.DataAccess;
using DataBooster.DbWebApi.DataAccess;

namespace DataBooster.DbWebApi
{
	public static partial class WebApiExtensions
	{
		public static int DetectSpChanges(int elapsedMinutes)
		{
			string detectDdlChangesProc = ConfigHelper.DetectDdlChangesProc;

			if (string.IsNullOrEmpty(detectDdlChangesProc) || elapsedMinutes < 1)
			{
				SwitchDerivedParametersCache(false);
				return -1;
			}
			else
			{
				SwitchDerivedParametersCache(true);

				using (DbContext dbContext = new DbContext())
				{
					return dbContext.InvalidateAlteredSpFromCache(detectDdlChangesProc, TimeSpan.FromMinutes(elapsedMinutes));
				}
			}
		}

		private static bool _DerivedParametersCacheInPeriodicDetection = false;
		internal static bool DerivedParametersCacheInPeriodicDetection
		{
			get { return _DerivedParametersCacheInPeriodicDetection; }
		}

		private static void SwitchDerivedParametersCache(bool periodicDetection)
		{
			if (periodicDetection != _DerivedParametersCacheInPeriodicDetection)
			{
				_DerivedParametersCacheInPeriodicDetection = periodicDetection;

				DerivedParametersCache.ExpireInterval = periodicDetection ?
					DbWebApiOptions.DetectDdlChangesContract.CacheExpireIntervalWithDetection :
					DbWebApiOptions.DetectDdlChangesContract.CacheExpireIntervalWithoutDetection;
			}
		}
	}
}
