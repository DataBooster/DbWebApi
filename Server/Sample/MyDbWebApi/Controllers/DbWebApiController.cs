using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Collections.Generic;
using DataBooster.DbWebApi;
using Newtonsoft.Json.Linq;

namespace MyDbWebApi.Controllers
{
	//	[DbWebApiAuthorize]
	[EnableCors(origins: "http://www.example.com", headers: "*", methods: "*", SupportsCredentials = true)]
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
			IDictionary<string, object> inputDict = Request.GatherInputParameters(parameters);

			if (!string.IsNullOrEmpty(ConfigHelper.UserNameReservedParameter) && User != null && User.Identity != null)
			{
				string userName = User.Identity.Name;

				if (!string.IsNullOrEmpty(userName))
					inputDict[ConfigHelper.UserNameReservedParameter] = userName;
			}

			return this.ExecuteDbApi(sp, inputDict);
		}

		[AcceptVerbs("POST", "PUT")]
		public HttpResponseMessage BulkExecute(string sp, List<Dictionary<string, object>> listOfParametersDict)
		{
			Request.BulkGatherInputParameters(listOfParametersDict);

			if (!string.IsNullOrEmpty(ConfigHelper.UserNameReservedParameter) && User != null && User.Identity != null)
			{
				string userName = User.Identity.Name;

				if (!string.IsNullOrEmpty(userName))
					foreach (var inputDict in listOfParametersDict)
						inputDict[ConfigHelper.UserNameReservedParameter] = userName;
			}

			return this.BulkExecuteDbApi(sp, listOfParametersDict);
		}

		#endregion	// Approach 2
	}
}
