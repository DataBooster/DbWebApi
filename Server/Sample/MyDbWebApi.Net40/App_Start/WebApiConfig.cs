using System.Web.Http;
using DbParallel.DataAccess;
using DataBooster.DbWebApi;
using MyDbWebApi.Handlers;

namespace MyDbWebApi
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
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
			config.MessageHandlers.Add(new CorsHandler());

		//	DbWebApiOptions.DefaultPropertyNamingConvention = PropertyNamingConvention.PascalCase;
		}
	}
}
