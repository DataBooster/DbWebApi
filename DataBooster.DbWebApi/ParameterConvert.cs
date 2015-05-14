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
				return jArrayValue.ToObject<DataTable>();

			if (first is JValue)
				return jArrayValue.ToObject<object[]>().NormalizeNumericArray();

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

			if (rawValue is IConvertible || rawValue is DataTable || rawValue is DbDataReader)
				return rawValue;

			JArray jArrayValue = rawValue as JArray;

			if (jArrayValue != null)
				return jArrayValue.AsParameterValue();

			Array arrayValue = rawValue as Array;

			if (arrayValue != null)
			{
				if (arrayValue.Length == 0)
					return null;

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

		/// <summary>
		/// <para>Normalize an object[] (promiscuous element numeric types) to a most compatible primitive type</para>
		/// <para>For example, object[] {0, 10000L, 3.14, 0.618m} ==> decimal[] {0m, 10000m, 3.14m, 0.618m}</para>
		/// </summary>
		/// <param name="rawArray">A promiscuous types' numeric array</param>
		/// <returns>A normalized new array if all elements are numeric, or just the rawArray itself if contains any non-numeric element.</returns>
		public static Array NormalizeNumericArray(this object[] rawArray)
		{
			Type compatibleType = GetElementType(rawArray);

			if (compatibleType != null && compatibleType != typeof(object))
			{
				Array newArray = Array.CreateInstance(compatibleType, rawArray.Length);
				Array.Copy(rawArray, newArray, rawArray.Length);
				return newArray;
			}

			return rawArray;
		}

		private static Type GetElementType(Array arrayValue)
		{
			int weight, maxWeight = 0;
			Type mostCompatibleType = null;

			foreach (object element in arrayValue)
			{
				weight = WeighNumericType(element);

				if (weight < 0)
					return null;
				else if (weight > maxWeight)
				{
					mostCompatibleType = element.GetType();
					maxWeight = weight;
				}
			}

			return mostCompatibleType;
		}

		private static int WeighNumericType(object numericObject)
		{
			if (numericObject == null || Convert.IsDBNull(numericObject))
				return 0;

			//	if (numericObject is bool)
			//		return 1;

			if (numericObject is sbyte)
				return 2;

			if (numericObject is byte)
				return 3;

			//	if (numericObject is char)
			//		return 4;

			if (numericObject is short)
				return 5;

			if (numericObject is ushort)
				return 6;

			if (numericObject is int)
				return 7;

			if (numericObject is uint)
				return 8;

			if (numericObject is long)
				return 9;

			if (numericObject is ulong)
				return 10;

			if (numericObject is float)
				return 11;

			if (numericObject is double)
				return 12;

			if (numericObject is decimal)
				return 13;

			return -1;
		}
	}
}
