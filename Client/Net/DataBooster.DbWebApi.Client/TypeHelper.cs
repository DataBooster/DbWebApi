#region https://databooster.codeplex.com/SourceControl/latest#DataAccess/ParameterConvert.cs
using System;

namespace DataBooster.DbWebApi.Client
{
	internal static class TypeHelper
	{
		private static int WeighNumericType(object numericObject)
		{
			return (numericObject == null) ? 0 : WeighNumericType(numericObject.GetType());
		}

		private static int WeighNumericType(Type numericType)
		{
			switch (Type.GetTypeCode(numericType))
			{
				case TypeCode.DBNull: return 0;
				//case TypeCode.Boolean: return 1;
				case TypeCode.SByte: return 2;
				case TypeCode.Byte: return 3;
				//case TypeCode.Char: return 4;
				case TypeCode.Int16: return 5;
				case TypeCode.UInt16: return 6;
				case TypeCode.Int32: return 7;
				case TypeCode.UInt32: return 8;
				case TypeCode.Int64: return 9;
				case TypeCode.UInt64: return 10;
				case TypeCode.Single: return 11;
				case TypeCode.Double: return 12;
				case TypeCode.Decimal: return 13;
				default: return -1;
			}
		}
	}
}

#endregion
