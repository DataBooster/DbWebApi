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

		private IList<IDictionary<string, object>> _BulkParameters;
		public IList<IDictionary<string, object>> BulkParameters
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
			_BulkParameters = jBulkParameters.ToObject<Dictionary<string, object>[]>();
		}

		public static implicit operator InputParameters(JObject jParameters)
		{
			return new InputParameters(jParameters);
		}

		public static implicit operator InputParameters(JArray jBulkParameters)
		{
			return new InputParameters(jBulkParameters);
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
				return xValue;
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
			{
				_ForBulkExecuting = true;
				_BulkParameters = ReadXmlArray(xRoot);
			}
			else
			{
				_ForBulkExecuting = false;
				_Parameters = ReadXml(xRoot);
			}
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
