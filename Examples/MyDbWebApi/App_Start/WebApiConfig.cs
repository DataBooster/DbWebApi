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
			#region Solution 1: Auto-detect a post request body. Invoking BulkExecute if input parameters are wrapped in an arrray; or invoking Execute if input parameters are wrapped in a usual dictionary.

			config.Routes.MapHttpRoute(
				name: "DbWebApi",
				routeTemplate: "{sp}/{ext}",
				defaults: new { controller = "DbWebApi", action = "DynExecute", ext = RouteParameter.Optional },
				constraints: new { ext = @"|json|xml|csv|xlsx|jsonp|razor" }
			);

			#endregion	// Solution 1

			/*
			#region Solution 2: Separate BulkExecute action from Execute action

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
				constraints: new { ext = @"|json|jsonp|xml" }
			);

			#endregion	// Solution 2
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
