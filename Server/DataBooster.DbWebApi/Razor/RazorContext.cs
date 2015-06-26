// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Collections.Generic;
using DbParallel.DataAccess;
using RazorEngine;
using RazorEngine.Templating;

namespace DataBooster.DbWebApi.Razor
{
	class RazorContext
	{
		private readonly SerializableResponseData _Model;
		public SerializableResponseData Model { get { return _Model; } }

		private string _RazorTemplate;
		public string RazorTemplate { get { return _RazorTemplate; } }

		private readonly Encoding _RazorEncoding;
		private readonly Language _RazorLanguage;

		public RazorContext(StoredProcedureResponse spResponse, IDictionary<string, object> razorParameters)
		{
			if (razorParameters == null)
				throw new ArgumentNullException("razorParameters");

			_RazorTemplate = TryGetParameter(razorParameters, DbWebApiOptions.QueryStringContract.RazorTemplateParameterName);

			if (string.IsNullOrEmpty(_RazorTemplate))
				throw new ArgumentNullException(DbWebApiOptions.QueryStringContract.RazorTemplateParameterName);

			string encoding = TryGetParameter(razorParameters, DbWebApiOptions.QueryStringContract.RazorEncodingParameterName);

			if (string.IsNullOrEmpty(encoding) || !Enum.TryParse(encoding, true, out _RazorEncoding))
				_RazorEncoding = DbWebApiOptions.DefaultRazorEncoding;

			string language = TryGetParameter(razorParameters, DbWebApiOptions.QueryStringContract.RazorLanguageParameterName);

			if (string.IsNullOrEmpty(language) || !Enum.TryParse(language, true, out _RazorLanguage))
				_RazorLanguage = DbWebApiOptions.DefaultRazorLanguage;

			_Model = new SerializableResponseData(spResponse);

			ResolveRazorTemplate();
		}

		private bool ResolveRazorTemplate()
		{
			if (_RazorTemplate.Length > 116)
				return false;

			string strTemplate = TryGetValueIgnoreCase(_Model.OutputParameters, _RazorTemplate) as string;

			if (string.IsNullOrEmpty(strTemplate))
				return false;

			_RazorTemplate = strTemplate;
			return true;
		}

		private object TryGetValueIgnoreCase(IDictionary<string, object> caseSensitiveDictionary, string key)
		{
			object outValue;

			if (caseSensitiveDictionary.TryGetValue(key, out outValue))
				return outValue;

			foreach (var kvp in caseSensitiveDictionary)
				if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
					return kvp.Value;

			return null;
		}

		private string TryGetParameter(IDictionary<string, object> razorParameters, string parameterName)
		{
			object obj;

			if (razorParameters.TryGetValue(parameterName, out obj))
				return obj as string;
			else
				return null;
		}

		public IsolatedRazorEngineService.IConfigCreator GetConfigCreator()
		{
			return new IsolatedRazorEngineService.LanguageEncodingConfigCreator(_RazorLanguage, _RazorEncoding);
		}
	}
}
