using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using DataBooster.DbWebApi;
using Newtonsoft.Json.Linq;

namespace MyDbWebApi.Controllers
{
	[DbWebApiAuthorize]
	public class DbWebApiController : ApiController
	{
		#region Approach 1: Auto-detect a post request body. Invoking BulkExecute if sets of input parameters are wrapped in an arrray; or invoking Execute if input parameters are wrapped in a single dictionary.

		[AcceptVerbs("GET", "POST", "PUT", "DELETE", "OPTIONS")]
		public HttpResponseMessage DynExecute(string sp, JContainer requestBody)
		{
			if (requestBody == null)
				return Execute(sp, null);

			JObject parameters = requestBody as JObject;

			if (parameters != null)
				return Execute(sp, parameters.ToObject<Dictionary<string, object>>());

			JArray bulkParameters = requestBody as JArray;

			if (bulkParameters != null)
			{
				List<Dictionary<string, object>> listOfDicts = bulkParameters.ToObject<List<Dictionary<string, object>>>();

				if (listOfDicts != null && listOfDicts.Count > 0)
					return BulkExecute(sp, listOfDicts);
				else
					return Request.CreateResponse(HttpStatusCode.NoContent);
			}

			return Request.CreateResponse(HttpStatusCode.BadRequest);
		}

		#endregion	// Approach 1

		#region Approach 2: Separate BulkExecute action from Execute action

		[AcceptVerbs("GET", "POST", "PUT", "DELETE", "OPTIONS")]
		public HttpResponseMessage Execute(string sp, Dictionary<string, object> parameters)
		{
			return this.ExecuteDbApi(sp, Request.GatherInputParameters(parameters));
		}

		[AcceptVerbs("POST", "PUT")]
		public HttpResponseMessage BulkExecute(string sp, List<Dictionary<string, object>> listOfParametersDict)
		{
			Request.BulkGatherInputParameters(listOfParametersDict);
			return this.BulkExecuteDbApi(sp, listOfParametersDict);
		}

		#endregion	// Approach 2
	}
}
