// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataBooster.DbWebApi
{
	[XmlSchemaProvider(null, IsAny = true)]
	[JsonConverter(typeof(InputParametersJsonConverter))]
	public class InputParameters : IXmlSerializable
	{
		private const string _NsNet = "http://schemas.microsoft.com/2003/10/Serialization/";
		private static readonly XNamespace _XNsXsi = XmlSchema.InstanceNamespace;
		private static readonly XName _XnType = _XNsXsi + "type";
		private static readonly XName _XnNil = _XNsXsi + "nil";

		public bool ForBulkExecuting
		{
			get { return (_BulkParameters != null); }
		}

		private IDictionary<string, object> _Parameters;
		public IDictionary<string, object> Parameters
		{
			get
			{
				return _Parameters;
			}
			protected set
			{
				_BulkParameters = null;
				_Parameters = value;
			}
		}

		private IList<IDictionary<string, object>> _BulkParameters;
		public IList<IDictionary<string, object>> BulkParameters
		{
			get
			{
				return _BulkParameters;
			}
			protected set
			{
				_Parameters = null;
				_BulkParameters = value;
			}
		}

		private InputParameters()
		{
		}

		public InputParameters(JObject jParameters)
		{
			if (jParameters != null)
			{
				Parameters = jParameters.ToObject<Dictionary<string, object>>();
				NormalizeValues(Parameters);
			}
		}

		public InputParameters(JArray jBulkParameters)
		{
			if (jBulkParameters != null)
			{
				BulkParameters = jBulkParameters.ToObject<Dictionary<string, object>[]>();

				foreach (var ps in BulkParameters)
					NormalizeValues(ps);
			}
		}

		public InputParameters(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
		{
			if (nameValuePairs != null)
				Parameters = nameValuePairs.NameValuePairsToDictionary();
		}

		public InputParameters(IDictionary<string, object> parametersDictionary)
		{
			if (parametersDictionary != null)
				Parameters = parametersDictionary;
		}

		public InputParameters(IDictionary<string, object>[] bulkParametersDictionaries)
		{
			if (bulkParametersDictionaries != null)
				BulkParameters = bulkParametersDictionaries;
		}

		private IDictionary<string, object> ReadXml(XElement xContainer)
		{
			var dynObject = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			string name;

			foreach (XElement x in xContainer.Elements())
			{
				name = x.Name.LocalName;

				if (dynObject.ContainsKey(name) == false)
					dynObject.Add(name, ReadElementValue(x));
			}

			XNamespace ns = xContainer.Name.Namespace;
			var attributes = xContainer.Attributes().Where(a => a.Name.Namespace == ns);

			foreach (XAttribute a in attributes)
			{
				name = a.Name.LocalName;

				if (dynObject.ContainsKey(name) == false)
					if (a.Value.Length == 0)
						dynObject.Add(name, DBNull.Value);
					else
						dynObject.Add(name, a.Value);
			}

			return dynObject;
		}

		private object ReadElementValue(XElement xValue)
		{
			XAttribute nilAttribute = xValue.Attribute(_XnNil);

			if (nilAttribute != null && (bool)nilAttribute)
				return DBNull.Value;

			XAttribute typeAttribute = xValue.Attribute(_XnType);

			if (typeAttribute != null)
				return ReadXsdValue(xValue);

			if (xValue.Attributes().Any(a => a.Name.LocalName == "Type" && a.Name.NamespaceName.StartsWith(_NsNet)))
				return ReadNetValue(xValue);

			if (IsXmlArray(xValue))
				return ReadXmlArray(xValue);

			if (xValue.HasElements)
				return ReadXml(xValue);
			else
				return xValue.Value;
		}

		private object ReadXsdValue(XElement xValue)
		{
			var xsdDataContractSerializer = new DataContractSerializer(typeof(object));
			return xsdDataContractSerializer.ReadObject(xValue.CreateReader(), false);
		}

		private object ReadNetValue(XElement xValue)
		{
			var netDataContractSerializer = new NetDataContractSerializer();
			return netDataContractSerializer.ReadObject(xValue.CreateReader(), false);
		}

		private IDictionary<string, object>[] ReadXmlArray(XElement xContainer)
		{
			var children = xContainer.Elements().ToList();
			var dynObjects = new IDictionary<string, object>[children.Count];

			for (int i = 0; i < children.Count; i++)
				dynObjects[i] = ReadXml(children[i]);

			return dynObjects;
		}

		private bool IsXmlArray(XElement xContainer)
		{
			XName lastName = null;

			if (xContainer.HasElements == false)
				return false;

			foreach (XElement x in xContainer.Elements())
			{
				if (x.HasElements == false && x.Attributes().Any(a => a.Name.Namespace == x.Name.Namespace) == false)
					return false;

				if (lastName != null)
					if (x.Name != lastName)
						return false;

				lastName = x.Name;
			}

			return true;
		}

		private IDictionary<string, object> NormalizeValues(IDictionary<string, object> dic)
		{
			var list = dic.ToList();

			foreach (var p in list)
			{
				var ja = p.Value as JArray;
				if (ja != null)
				{
					dic[p.Key] = NormalizeValues(ja.ToObject<object[]>());
					continue;
				}

				var jo = p.Value as JObject;
				if (jo != null)
				{
					dic[p.Key] = NormalizeValues(jo.ToObject<Dictionary<string, object>>());
					continue;
				}

				NormalizeValueFurther(p.Value);
			}

			return dic;
		}

		private object[] NormalizeValues(object[] array)
		{
			object item;

			for (int i = 0; i < array.Length; i++)
			{
				item = array[i];

				var ja = item as JArray;
				if (ja != null)
				{
					array[i] = NormalizeValues(ja.ToObject<object[]>());
					continue;
				}

				var jo = item as JObject;
				if (jo != null)
				{
					array[i] = NormalizeValues(jo.ToObject<Dictionary<string, object>>());
					continue;
				}

				NormalizeValueFurther(item);
			}

			return array;
		}

		private void NormalizeValueFurther(object itemValue)
		{
			var oa = itemValue as object[];
			if (oa != null)
			{
				NormalizeValues(oa);
				return;
			}

			var dic = itemValue as IDictionary<string, object>;
			if (dic != null)
			{
				NormalizeValues(dic);
				return;
			}
		}

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			XElement xRoot = XNode.ReadFrom(reader) as XElement;

			if (xRoot == null)
				return;

			if (IsXmlArray(xRoot))
				BulkParameters = ReadXmlArray(xRoot);
			else
				Parameters = ReadXml(xRoot);
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
