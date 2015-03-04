// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace DataBooster.DbWebApi.Client
{
	public class InputParameterDictionary : Dictionary<string, IConvertible>
	{
		public InputParameterDictionary()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		public InputParameterDictionary(Dictionary<string, IConvertible> dictionary)
			: base(dictionary, StringComparer.OrdinalIgnoreCase)
		{
		}

		public InputParameterDictionary(Dictionary<string, object> dictionary)
			: base(dictionary.Count, StringComparer.OrdinalIgnoreCase)
		{
			IConvertible val;

			foreach (var pair in dictionary)
			{
				val = pair.Value as IConvertible;

				if (val == null)
					throw new ArgumentOutOfRangeException(pair.Key);
				else
					Add(pair.Key, val);
			}
		}

		public InputParameterDictionary(object anonymousTypeInstance)
			: base(StringComparer.OrdinalIgnoreCase)
		{
			if (anonymousTypeInstance == null)
				return;

			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(anonymousTypeInstance);
			IConvertible val;

			foreach (PropertyDescriptor prop in properties)
			{
				val = prop.GetValue(anonymousTypeInstance) as IConvertible;

				if (val == null)
					throw new ArgumentOutOfRangeException(prop.Name);
				else
					Add(prop.Name, val);
			}
		}
	}
}
