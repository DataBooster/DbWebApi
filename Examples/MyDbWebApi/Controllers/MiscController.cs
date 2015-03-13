using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MyDbWebApi.Controllers
{
	public class MiscController : ApiController
	{
		public HttpResponseMessage Get(string method)
		{
			if (method.Equals("WhoAmI", StringComparison.OrdinalIgnoreCase))
				return Request.CreateResponse(HttpStatusCode.OK, WhoAmI());
			else
				return Request.CreateErrorResponse(HttpStatusCode.NotImplemented, string.Format("The request method ({0}) is invalid."));
		}

		private string WhoAmI()
		{
			if (User != null)
				if (User.Identity != null)
				{
					string userName = User.Identity.Name;

					if (!string.IsNullOrEmpty(userName))
						return userName;
				}

			return "?";
		}
	}
}
