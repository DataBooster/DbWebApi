using System.Web.Http;
using DataBooster.DbWebApi;
using DbWebApi.OwinSample.Controllers;

namespace DbWebApi.OwinSample.Filters
{
	public class DbWebApiAuthorizeAttribute : AuthorizeAttribute
	{
		private static IDbWebApiAuthorization _DbWebApiAuthorization;

		public static void RegisterWebApiAuthorization<T>() where T : IDbWebApiAuthorization, new()
		{
			if (_DbWebApiAuthorization == null)
				_DbWebApiAuthorization = new T();
		}

		/// <param name="actionContext">The context.</param>
		/// <returns>true if the control is authorized; otherwise, false.</returns>
		protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
		{
			var userIdentity = (actionContext.ControllerContext.Controller as DbWebApiController).User.Identity;

			if (userIdentity == null || userIdentity.IsAuthenticated == false || string.IsNullOrEmpty(userIdentity.Name))
				return false;

			string user = userIdentity.Name;
			string sp = actionContext.ControllerContext.RouteData.Values["sp"] as string;

			return _DbWebApiAuthorization.IsAuthorized(user, sp);
		}
	}
}