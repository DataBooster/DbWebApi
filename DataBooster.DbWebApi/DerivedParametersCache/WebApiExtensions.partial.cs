// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Web.Http;
using DataBooster.DbWebApi.DataAccess;

namespace DataBooster.DbWebApi
{
	public static partial class WebApiExtensions
	{
		public static int DetectSpChanges(this ApiController apiController, int elapsedMinutes)
		{
			return DetectSpChanges(elapsedMinutes);
		}

		public static int DetectSpChanges(int elapsedMinutes)
		{
			string detectDdlChangesProc = ConfigHelper.DetectDdlChangesProc;

			if (string.IsNullOrEmpty(detectDdlChangesProc) || elapsedMinutes < 1)
				return -1;

			using (DbContext dbContext = new DbContext())
			{
				return dbContext.InvalidateAlteredSpFromCache(detectDdlChangesProc, TimeSpan.FromMinutes(elapsedMinutes));
			}
		}
	}
}
