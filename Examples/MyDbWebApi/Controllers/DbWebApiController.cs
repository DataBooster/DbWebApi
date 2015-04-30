using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using DataBooster.DbWebApi;

namespace MyDbWebApi.Controllers
{
	[DbWebApiAuthorize]
	public class DbWebApiController : ApiController
	{
		[AcceptVerbs("GET", "POST", "PUT", "DELETE", "OPTIONS")]
		public HttpResponseMessage Execute(string sp, Dictionary<string, object> parameters)
		{
			return this.ExecuteDbApi(sp, Request.GatherInputParameters(parameters));
		}

		[AcceptVerbs("POST", "PUT")]
		public HttpResponseMessage BulkExecute(string sp, List<IDictionary<string, object>> listOfParametersDict)
		{
			Request.BulkGatherInputParameters(listOfParametersDict);
			return this.BulkExecuteDbApi(sp, listOfParametersDict);
		}
	}
}
