using System.Net.Http;
using System.Web.Http;
using DataBooster.DbWebApi;

namespace MyDbWebApi.Controllers
{
	[DbWebApiAuthorize]
	public class DbWebApiController : ApiController
	{
		// Auto-detect a post request body. Invoking BulkExecute if sets of input parameters are wrapped in an arrray; or invoking Execute if input parameters are wrapped in a single dictionary.
		[AcceptVerbs("GET", "POST", "PUT", "DELETE", "OPTIONS")]
		public HttpResponseMessage DynExecute(string sp, InputParameters dynParameters)
		{
			if (dynParameters == null)
				dynParameters = new InputParameters(Request);
			else
				dynParameters.SupplementQueryString(Request);

			if (!string.IsNullOrEmpty(ConfigHelper.UserNameReservedParameter) && User != null && User.Identity != null)
			{
				string userName = User.Identity.Name;

				if (!string.IsNullOrEmpty(userName))
					dynParameters.SetParameter(ConfigHelper.UserNameReservedParameter, userName);
			}

			return this.DynExecuteDbApi(sp, dynParameters);
		}
	}
}
