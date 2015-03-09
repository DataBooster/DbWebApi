using System.Globalization;
using Newtonsoft.Json.Serialization;

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
			if (string.IsNullOrEmpty(propertyName) || propertyName.Length == 1)
				return propertyName;

			char[] pascalChars = new char[propertyName.Length];
			int cntUpper = 0, cntLower = 0, lenFragment = 0, lenPascal = 0;

			foreach (char c in propertyName)
			{
				if (char.IsUpper(c))
				{
					pascalChars[lenPascal] = (lenFragment == 0) ? c : char.ToLower(c, CultureInfo.InvariantCulture);
					lenPascal++;
					lenFragment++;
					cntUpper++;
				}
				else if (char.IsLower(c))
				{
					pascalChars[lenPascal] = (lenFragment == 0) ? char.ToUpper(c, CultureInfo.InvariantCulture) : c;
					lenPascal++;
					lenFragment++;
					cntLower++;
				}
				else if (char.IsPunctuation(c) || char.IsWhiteSpace(c))
				{
					lenFragment = 0;
				}
				else
				{
					pascalChars[lenPascal] = c;
					lenPascal++;
					lenFragment = 0;
				}
			}

			if (lenPascal == 0)
				return string.Empty;
			else
				if (_CamelCase && char.IsUpper(pascalChars[0]))
					pascalChars[0] = char.ToLower(pascalChars[0], CultureInfo.InvariantCulture);

			return (cntUpper > 0 && cntLower > 0) ? propertyName : new string(pascalChars, 0, lenPascal);
		}
	}
}
