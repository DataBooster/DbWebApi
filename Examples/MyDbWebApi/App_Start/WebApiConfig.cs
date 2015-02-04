using System;
using System.Web.Http;
using DataBooster.DbWebApi;

namespace MyDbWebApi
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				name: "DbWebApi",
				routeTemplate: "{sp}/{ext}",
				defaults: new { controller = "DbWebApi", ext = RouteParameter.Optional },
				constraints: new { ext = @"|json|xml|csv" }
			);

			config.SupportCsvMediaType();
			DbWebApiOptions.DerivedParametersCacheExpireInterval = new TimeSpan(0, 15, 0);
		}
	}
}
