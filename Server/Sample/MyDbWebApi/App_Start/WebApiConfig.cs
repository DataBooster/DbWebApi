using System.Web.Http;
using System.Web.Http.Cors;
using DataBooster.DbWebApi;

namespace MyDbWebApi
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services

			EnableCors(config);

			// Web API routes

			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DbWebApi",
				routeTemplate: "{sp}/{ext}",
				defaults: new { controller = "DbWebApi", action = "DynExecute", ext = RouteParameter.Optional },
				constraints: new { ext = @"|json|bson|xml|csv|xlsx|jsonp|razor" }
			);

			config.Routes.MapHttpRoute(
				name: "MiscApi",
				routeTemplate: "api/{controller}/{action}"
			);

			config.RegisterDbWebApi();
#if DEBUG
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
#endif
		//	DbWebApiOptions.DefaultPropertyNamingConvention = PropertyNamingConvention.PascalCase;

			DbWebApiAuthorizeAttribute.RegisterWebApiAuthorization<MyDbWebApiAuthorization>();
		}

		private static void EnableCors(HttpConfiguration config)
		{
			if (!string.IsNullOrEmpty(ConfigHelper.CorsOrigins))
			{
				var cors = new EnableCorsAttribute(ConfigHelper.CorsOrigins, "*", "*");

				cors.SupportsCredentials = ConfigHelper.SupportsCredentials;

				if (ConfigHelper.PreflightMaxAge > 0L)
					cors.PreflightMaxAge = ConfigHelper.PreflightMaxAge;

				config.EnableCors(cors);
			}
		}
	}
}
