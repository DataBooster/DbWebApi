using System.Web.Http;

namespace MyDbWebApi.Controllers
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
	}
}
