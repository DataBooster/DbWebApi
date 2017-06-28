using System.Net.Http;
using System.Web.Http;
using DataBooster.DbWebApi;
using DbWebApi.OwinSample.Filters;

namespace DbWebApi.OwinSample.Controllers
{
	[DbWebApiAuthorize]
	public class DbWebApiController : ApiController
	{
		/// <summary>
		/// Auto-detect a post request body. Invoking BulkExecute if sets of input parameters are wrapped in an arrray; or invoking Execute if input parameters are wrapped in a single dictionary.
		/// </summary>
		/// <param name="sp">Stored Procedure's fully qualified name</param>
		/// <param name="allParameters">Auto-binding from the request body</param>
		/// <returns>A complete HttpResponseMessage contains result data returned from the database</returns>
		[AcceptVerbs("GET", "POST", "PUT", "DELETE")]
		public HttpResponseMessage DynExecute(string sp, InputParameters allParameters)
		{
			allParameters = InputParameters.SupplementFromQueryString(allParameters, Request);	// Supplement input parameters from URI query string.

			SetUserName(allParameters);							// Set the conventional User Name Parameter if configured.

			return this.DynExecuteDbApi(sp, allParameters);		// The main entry point to call the DbWebApi.
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
