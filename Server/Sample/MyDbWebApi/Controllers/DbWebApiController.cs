using System.Net.Http;
using System.Web.Http;
using DataBooster.DbWebApi;

namespace MyDbWebApi.Controllers
{
	[DbWebApiAuthorize]
	public class DbWebApiController : ApiController
	{
		/// <summary>
		/// Auto-detect a post request body. Invoking BulkExecute if sets of input parameters are wrapped in an arrray; or invoking Execute if input parameters are wrapped in a single dictionary.
		/// </summary>
		/// <param name="sp">Stored Procedure's fully qualified name</param>
		/// <param name="dynParameters">Auto-binding from the request body</param>
		/// <returns></returns>
		[AcceptVerbs("GET", "POST", "PUT", "DELETE", "OPTIONS")]
		public HttpResponseMessage DynExecute(string sp, InputParameters dynParameters)
		{
			if (dynParameters == null)
				dynParameters = new InputParameters(Request);	// Gather input parameters from the URI (query string) if not in the request body.
			else
				dynParameters.SupplementQueryString(Request);	// Accept URI query string as as a supplementary.

			SetUserName(dynParameters);							// Set the conventional User Name Parameter if configured.

			return this.DynExecuteDbApi(sp, dynParameters);		// The main entry point to call the DbWebApi.
		}

		private void SetUserName(InputParameters dynParameters)
		{
			if (!string.IsNullOrEmpty(ConfigHelper.UserNameReservedParameter) && User != null && User.Identity != null)
			{
				string userName = User.Identity.Name;

				if (!string.IsNullOrEmpty(userName))
					dynParameters.SetParameter(ConfigHelper.UserNameReservedParameter, userName);
			}
		}
	}
}
