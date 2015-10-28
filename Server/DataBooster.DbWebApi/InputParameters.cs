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

		private void ReadXml(XElement xNode, IDictionary<string, object> dynObject)
		{
			XNamespace ns = xNode.Name.Namespace;
			string name;

			foreach (XElement x in xNode.Elements())
			{
				name = x.Name.LocalName;

				if (dynObject.ContainsKey(name))
					continue;

				if (x.HasElements)
				{
					Dictionary<string, object> subDic = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					dynObject.Add(name, subDic);
				}
				else
				{
					dynObject.Add(name, x.Value);
				}
			}

			var attributes = xNode.Attributes().Where(a => a.Name.Namespace == ns);

			foreach (XAttribute a in attributes)
			{
				name = a.Name.LocalName;
				if (dynObject.ContainsKey(name) == false)
					dynObject.Add(name, a.Value);
			}
		}

		private bool IsBulkXml(XElement xRoot)
		{
			XName lastName = null;
			int count = 0;

			foreach (XElement x in xRoot.Elements())
			{
				if (x.HasElements == false && x.Attributes().Any(a => a.Name.Namespace == x.Name.Namespace) == false)
					return false;

				if (lastName != null)
					if (x.Name != lastName)
						return false;

				lastName = x.Name;
				count++;
			}

			return count > 0;
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

			if (IsBulkXml(xRoot))
			{
				_ForBulkExecuting = true;
				_BulkParameters = new List<Dictionary<string, object>>();
				Dictionary<string, object> dic;

				foreach (XElement x in xRoot.Elements())
				{
					dic = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					ReadXml(xRoot, dic);
					_BulkParameters.Add(dic);
				}
			}
			else
			{
				_ForBulkExecuting = false;
				_Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				ReadXml(xRoot, _Parameters);
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
