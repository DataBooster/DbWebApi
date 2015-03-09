using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace DataBooster.DbWebApi
{
	/// <summary>
	/// Resolves member mappings for a type, De-underscore property names.
	/// </summary>
	public class DeunderscorePropertyNamesContractResolver : DefaultContractResolver
	{
		public DeunderscorePropertyNamesContractResolver()
			: base(true)
		{
		}

		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Name of the property.</returns>
		protected override string ResolvePropertyName(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
				return propertyName;

			int cntUpper = 0, cntLower = 0;

			return propertyName;
		}
	}
}
