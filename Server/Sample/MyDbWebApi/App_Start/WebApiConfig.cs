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

			if (!string.IsNullOrEmpty(ConfigHelper.CorsOrigins))
			{
				var cors = new EnableCorsAttribute(ConfigHelper.CorsOrigins, "*", "*");
				cors.SupportsCredentials = ConfigHelper.SupportsCredentials;
				config.EnableCors(cors);
			}

			// Web API routes

			config.MapHttpAttributeRoutes();

			#region Approach 1: Auto-detect a post request body. Invoking BulkExecute if sets of input parameters are wrapped in an arrray; or invoking Execute if input parameters are wrapped in a single dictionary.

			config.Routes.MapHttpRoute(
				name: "DbWebApi",
				routeTemplate: "{sp}/{ext}",
				defaults: new { controller = "DbWebApi", action = "DynExecute", ext = RouteParameter.Optional },
				constraints: new { ext = @"|json|xml|csv|xlsx|jsonp|razor" }
			);

			#endregion	// Approach 1

			/*
			#region Approach 2: Separate BulkExecute action from Execute action

			config.Routes.MapHttpRoute(
				name: "DbWebApi",
				routeTemplate: "{sp}/{ext}",
				defaults: new { controller = "DbWebApi", action = "Execute", ext = RouteParameter.Optional },
				constraints: new { ext = @"|json|xml|csv|xlsx|jsonp|razor" }
			);

			config.Routes.MapHttpRoute(
				name: "BulkApi",
				routeTemplate: "bulk/{sp}/{ext}",
				defaults: new { controller = "DbWebApi", action = "BulkExecute", ext = RouteParameter.Optional },
				constraints: new { ext = @"|json|xml" }
			);

			#endregion	// Approach 2
			*/

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
	}
}
