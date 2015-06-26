// Copyright (c) 2015 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace DbParallel.DataAccess
{
	// This transverter class is to eliminate bulk duplicated "http://www.w3.org/2001/XMLSchema" in XML Response
	class ResponseRoot : IXmlSerializable
	{
		private StoredProcedureResponse _Content;

		public ResponseRoot()
		{
			_Content = null;
		}

		public ResponseRoot(StoredProcedureResponse content)
		{
			_Content = content;
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
			writer.WriteAttributeString("xmlns", "x", null, "http://www.w3.org/2001/XMLSchema");

			if (_Content != null)
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(StoredProcedureResponse));
				serializer.WriteObjectContent(writer, _Content);
			}
		}
	}
}
