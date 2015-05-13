// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace DataBooster.DbWebApi.Client
{
	public class InputParameterDictionary : Dictionary<string, object>
	{
		public InputParameterDictionary()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		public InputParameterDictionary(IDictionary<string, object> dictionary)
			: base(dictionary, StringComparer.OrdinalIgnoreCase)
		{
		}

		[Obsolete("This constructor is deprecated and will be removed in the next major release. Use InputParameterDictionary(IDictionary<string, object> dictionary) instead.", false)]
		public InputParameterDictionary(IDictionary<string, IConvertible> dictionary)
			: base(dictionary.ToDictionary(d => d.Key, d => d.Value as object, StringComparer.OrdinalIgnoreCase))
		{
		}

		public InputParameterDictionary(object anonymousTypeInstance)
			: base(StringComparer.OrdinalIgnoreCase)
		{
			if (anonymousTypeInstance == null)
				return;

			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(anonymousTypeInstance);

			foreach (PropertyDescriptor prop in properties)
				Add(prop.Name, prop.GetValue(anonymousTypeInstance));
		}
	}
}
