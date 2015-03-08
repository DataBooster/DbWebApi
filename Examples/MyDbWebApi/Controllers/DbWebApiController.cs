using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using DataBooster.DbWebApi;

namespace MyDbWebApi.Controllers
{
	public class DbWebApiController : ApiController
	{
		[DbWebApiAuthorize]
		[AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")]
		public HttpResponseMessage Execute(string sp, Dictionary<string, object> parameters)
		{
			return this.ExecuteDbApi(sp, Request.GatherInputParameters(parameters));
		}
	}
}
