using System.Web.Http;
using DataBooster.DbWebApi;
using MyDbWebApi.Handlers;

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
				constraints: new { ext = @"|json|xml|csv|xlsx" }
			);

			config.RegisterDbWebApi();
#if DEBUG
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
#endif
			config.MessageHandlers.Add(new CorsHandler());
		}
	}
}
