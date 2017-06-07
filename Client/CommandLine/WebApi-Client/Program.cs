// Copyright (c) 2017 Abel Cheng <abelcys@gmail.com> and other contributors.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Repository:	https://github.com/DataBooster/DbWebApi

using System;
using CommandLine;
using DataBooster.DbWebApi.Client;

namespace DataBooster.DbWebApi.Client.CmdUtility
{
	class Program
	{
		static int Main(string[] args)
		{
			AppConfiguration options = new AppConfiguration();

			try
			{
				if (Parser.Default.ParseArguments(args, options))
				{
					Console.Write(ExecWebApi(options, Console.In.ReadToEnd()));
					return 0;
				}
				else
					return 1;
			}
			catch (Exception e)
			{
				WriteException(e);
				return 2;
			}
		}

		static string ExecWebApi(AppConfiguration options, string inputBody)
		{
			using (DbWebApiClient restClient = string.IsNullOrEmpty(options.UserName) ? new DbWebApiClient() : new DbWebApiClient(options.UserName, options.Password))
			{
				if (!string.IsNullOrWhiteSpace(options.StrMethod))
					restClient.HttpMethod = options.Method;

				if (!string.IsNullOrWhiteSpace(options.StrAcceptType))
					restClient.AcceptMediaTypes.Add(options.AcceptType);

				if (options.TimeoutSec > 0)
					restClient.HttpClient.Timeout = options.Timeout;

				return restClient.ExecAsString(requestUri: options.Uri, content: inputBody, mediaType: options.ContentType);
			}
		}

		static void WriteException(Exception e)
		{
			if (e == null)
				return;

			if (!string.IsNullOrEmpty(e.Message))
				Console.Error.WriteLine("Exception Message: " + e.Message);
			Console.Error.WriteLine("Exception Type: " + e.GetType().FullName);
			if (!string.IsNullOrEmpty(e.StackTrace))
				Console.Error.WriteLine("Stack Trace: " + e.StackTrace);

			if (e.InnerException != null)
			{
				Console.Error.WriteLine(">>>> Inner Exception <<<<");
				WriteException(e.InnerException);
			}
		}
	}
}
