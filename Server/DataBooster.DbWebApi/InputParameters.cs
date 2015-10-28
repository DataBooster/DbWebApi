// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataBooster.DbWebApi
{
	[XmlRoot(Namespace = "")]
	[JsonConverter(typeof(InputParametersJsonConverter))]
	public class InputParameters : IXmlSerializable
	{
		private bool _ForBulkExecuting;
		public bool ForBulkExecuting
		{
			get { return _ForBulkExecuting; }
		}

		private IDictionary<string, object> _Parameters;
		public IDictionary<string, object> Parameters
		{
			get { return _Parameters; }
		}

		private IList<Dictionary<string, object>> _BulkParameters;
		public IList<Dictionary<string, object>> BulkParameters
		{
			get { return _BulkParameters; }
		}

		public InputParameters()
		{
			_ForBulkExecuting = false;
		}

		internal InputParameters(JObject jParameters)
		{
			_ForBulkExecuting = false;
			_Parameters = jParameters.ToObject<Dictionary<string, object>>();
		}

		internal InputParameters(JArray jBulkParameters)
		{
			_ForBulkExecuting = true;
			_BulkParameters = jBulkParameters.ToObject<List<Dictionary<string, object>>>();
		}

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}
	}

	internal class InputParametersJsonConverter : JsonConverter
	{

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken jToken = JToken.Load(reader);

			JArray bulkParameters = jToken as JArray;

			if (bulkParameters != null)
				return new InputParameters(bulkParameters);

			JObject jParameters = jToken as JObject;

			if (jParameters != null)
				return new InputParameters(jParameters);

			throw new JsonReaderException();
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(InputParameters).IsAssignableFrom(objectType);
		}
	}
}
