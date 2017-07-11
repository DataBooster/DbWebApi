using Owin;
using Microsoft.Owin;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Net.Http.Headers;
using DataBooster.DbWebApi;
using DbWebApi.OwinSample.Filters;

[assembly: OwinStartup(typeof(DbWebApi.OwinSample.Startup))]

namespace DbWebApi.OwinSample
{
	public class Startup
	{
		private static HttpServer _httpServer;

		static Startup()
		{
			HttpConfiguration config = new HttpConfiguration();

			EnableCors(config);
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
			config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
#if DEBUG
			config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
#endif
			//	DbWebApiOptions.DefaultPropertyNamingConvention = PropertyNamingConvention.PascalCase;

			DbWebApiAuthorizeAttribute.RegisterWebApiAuthorization<MyDbWebApiAuthorization>();

			_httpServer = new HttpServer(config);
		}

		public void Configuration(IAppBuilder app)
		{
			app.UseWebApi(_httpServer);
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
