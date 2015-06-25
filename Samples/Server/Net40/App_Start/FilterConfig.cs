using System.Web.Mvc;

namespace MyDbWebApi
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());

			DbWebApiAuthorizeAttribute.RegisterWebApiAuthorization<MyDbWebApiAuthorization>();
		}
	}
}