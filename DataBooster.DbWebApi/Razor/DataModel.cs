// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using DbParallel.DataAccess;

namespace DataBooster.DbWebApi.Razor
{
	[Serializable]
	public class DataModel
	{
		public List<List<BindableDynamicObject>> ResultSets { get; set; }
		public BindableDynamicObject OutputParameters { get; set; }
		public object ReturnValue { get; set; }

		public DataModel()
		{
		}

		public DataModel(StoredProcedureResponse spResponse)
		{
			if (spResponse == null)
				throw new ArgumentNullException("spResponse");

			ResultSets = spResponse.ResultSets;
			OutputParameters = spResponse.OutputParameters;
			ReturnValue = spResponse.ReturnValue;
		}
	}
}
