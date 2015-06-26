#region http://referencesource.microsoft.com/#System.Core/System/Linq/TypeHelper.cs

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace DataBooster.DbWebApi
{
	internal static class TypeHelper
	{
		internal static Type FindGenericType(this Type type, Type definition)
		{
			while (type != null && type != typeof(object))
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == definition)
					return type;

				if (definition.IsInterface)
				{
					foreach (Type itype in type.GetInterfaces())
					{
						Type found = FindGenericType(itype, definition);

						if (found != null)
							return found;
					}
				}

				type = type.BaseType;
			}
			return null;
		}

		internal static Type GetEnumerableElementType(this Type enumerableType)
		{
			Type ienumType = enumerableType.FindGenericType(typeof(IEnumerable<>));

			return (ienumType == null) ? null : ienumType.GetGenericArguments()[0];
		}
	}
}

#endregion
