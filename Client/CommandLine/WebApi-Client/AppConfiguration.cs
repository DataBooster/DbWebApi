// Copyright (c) 2017 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CommandLine;
using CommandLine.Text;

namespace DataBooster.DbWebApi.Client.CmdUtility
{
	class AppConfiguration
	{
		[Option('u', "Uri", Required = true, HelpText = "Specifies the Uniform Resource Identifier (URI) of the Internet resource to which the web request is sent.")]
		public string Uri { get; set; }

		[Option('m', "Method", DefaultValue = "Post", HelpText = "Specifies the method used for the web api request.")]
		public string StrMethod { get { return Method.Method; } set { Method = new HttpMethod(value); } }
		public HttpMethod Method { get; private set; }

		[Option('c', "ContentType", DefaultValue = "application/json", HelpText = "Specifies the content type of the web request.")]
		public string ContentType { get; set; }

		[Option('a', "AcceptType", HelpText = "Specifies the expected media type of the response.")]
		public string StrAcceptType { get { return AcceptType.MediaType; } set { AcceptType = new MediaTypeWithQualityHeaderValue(value); } }
		public MediaTypeWithQualityHeaderValue AcceptType { get; private set; }

		[Option('t', "TimeoutSec", DefaultValue = 0, HelpText = "Specifies how long the request can be pending before it times out. Enter a value in seconds.")]
		public int TimeoutSec { get { return (int)Timeout.TotalSeconds; } set { Timeout = TimeSpan.FromSeconds(value); } }
		public TimeSpan Timeout { get; private set; }

		[Option('n', "userName", HelpText = "Specifies the username if using Basic Authorization for the web request.")]
		public string UserName { get; set; }

		[Option('p', "Password", HelpText = "Specifies the password if using Basic Authorization for the web request.")]
		public string Password { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this,
			  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
