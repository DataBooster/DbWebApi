using System.Web.Http;
using DataBooster.DbWebApi;

namespace DbWebApi.OwinSample.Controllers
{
	public class MiscController : ApiController
	{
		[AcceptVerbs("GET", "POST")]
		public string WhoAmI()
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

		[AcceptVerbs("GET", "POST")]
		public int DetectSpChanges(int elapsedMinutes)
		{
			return WebApiExtensions.DetectSpChanges(elapsedMinutes);
		}
	}
}
