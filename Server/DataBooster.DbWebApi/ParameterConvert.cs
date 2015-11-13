// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using DbParallel.DataAccess;

namespace DataBooster.DbWebApi
{
	/// <summary>
	/// For supporting Table-Valued Parameter (SQL Server 2008+) and Oracle Associative Array Parameter
	/// </summary>
	public static class ParameterConvert
	{
		/// <summary>
		/// Convert a JArray value to an acceptable parameter type for SQL Server Table-Valued Parameter or Oracle Associative Array Parameter
		/// </summary>
		/// <param name="jArrayValue">A raw JSON value</param>
		/// <returns>A DataTable if the first child element is JObject; Or an object[] if the first child element is JValue; Otherwise the jArrayValue itself.</returns>
		public static object AsParameterValue(this JArray jArrayValue)
		{
			if (jArrayValue == null || !jArrayValue.HasValues)
				return null;

			var first = jArrayValue.First;

			if (first is JObject)
				return jArrayValue.ToObject<List<Dictionary<string, object>>>().ToDataTable();

			if (first is JValue)
				return jArrayValue.ToObject<object[]>()/*.NormalizeNumericArray()*/;

			return jArrayValue;
		}

		/// <summary>
		/// Check an input object is an acceptable parameter value type, convert if necessary.
		/// </summary>
		/// <param name="rawValue">A raw value object</param>
		/// <returns>An acceptable type parameter value</returns>
		public static object AsParameterValue(object rawValue)
		{
			if (rawValue == null)
				return DBNull.Value;

			if (rawValue is IConvertible || rawValue is DataTable || rawValue is DbDataReader || rawValue is byte[])
				return rawValue;

			JArray jArrayValue = rawValue as JArray;

			if (jArrayValue != null)
				return jArrayValue.AsParameterValue();

			Array arrayValue = rawValue as Array;

			if (arrayValue != null)
			{
				if (arrayValue.Length == 0)
					return null;	// null: not to send the parameter	-- To review for empty SQL Server Table-Valued Parameter and Oracle Associative Array Parameter

				if (arrayValue.GetValue(0) is IConvertible)
					return arrayValue;
			}

			Type elementType = rawValue.GetType().GetEnumerableElementType();

			if (elementType == null)
				return rawValue;

			#region When the rawValue is IEnumerable<T>

			if (typeof(IConvertible).IsAssignableFrom(elementType))
				return (rawValue as IEnumerable).AsOfType<IConvertible>().AsParameterValue();

			if (typeof(SqlDataRecord).IsAssignableFrom(elementType))
				return (rawValue as IEnumerable).AsOfType<SqlDataRecord>().AsParameterValue();

			if (typeof(IDictionary<string, object>).IsAssignableFrom(elementType))
				return (rawValue as IEnumerable).AsOfType<IDictionary<string, object>>().AsParameterValue();

			return (rawValue as IEnumerable).AsOfType<object>().AsParameterValue();

			#endregion
		}

		/// <summary>
		/// Pretreat input parameters dictionary, check each input object is an acceptable parameter value type, convert if necessary.
		/// </summary>
		/// <param name="rawParameters">A input parameters dictionary</param>
		/// <returns>A new dictionary that contains all pretreated input parameters</returns>
		public static IDictionary<string, object> PretreatInputDictionary(this IDictionary<string, object> rawParameters)
		{
			if (rawParameters == null || rawParameters.Count == 0)
				return rawParameters;
			else
				return rawParameters.ToDictionary(p => p.Key, p => AsParameterValue(p.Value), StringComparer.OrdinalIgnoreCase);
		}
	}
}
