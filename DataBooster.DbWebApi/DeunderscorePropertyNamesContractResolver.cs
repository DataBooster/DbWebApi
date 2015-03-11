// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Serialization;
using DbParallel.DataAccess;

namespace DataBooster.DbWebApi
{
	/// <summary>
	/// Resolves member mappings for a type, De-underscore property names.
	/// </summary>
	public class DeunderscorePropertyNamesContractResolver : DefaultContractResolver
	{
		private bool _CamelCase;

		public DeunderscorePropertyNamesContractResolver(bool camelCase = false)
			: base(true)
		{
			_CamelCase = camelCase;
		}

		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Name of the property.</returns>
		protected override string ResolvePropertyName(string propertyName)
		{
			return propertyName.DeunderscoreFieldName(_CamelCase);
		}
	}
}
