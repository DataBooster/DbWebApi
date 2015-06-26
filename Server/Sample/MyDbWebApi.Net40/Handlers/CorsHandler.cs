// Code based on: https://code.msdn.microsoft.com/CORS-support-in-ASPNET-Web-01e9980a/sourcecode?fileId=60420&pathId=1908491756

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyDbWebApi.Handlers
{
	public class CorsHandler : DelegatingHandler
	{
		const string Origin = "Origin";
		const string AccessControlRequestMethod = "Access-Control-Request-Method";
		const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
		const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
		const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
		const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			bool isCorsRequest = request.Headers.Contains(Origin);
			bool isPreflightRequest = request.Method == HttpMethod.Options;

			if (isCorsRequest)
			{
				string originDomain = request.Headers.GetValues(Origin).First();

				if (isPreflightRequest)
				{
					HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Headers.Add(AccessControlAllowOrigin, originDomain);

					string accessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();
					if (accessControlRequestMethod != null)
						response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);

					string requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));
					if (!string.IsNullOrEmpty(requestedHeaders))
						response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);

					TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
					tcs.SetResult(response);
					return tcs.Task;
				}
				else
					return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>(t =>
					{
						HttpResponseMessage resp = t.Result;
						resp.Headers.Add(AccessControlAllowOrigin, originDomain);
						return resp;
					});
			}
			else
				return base.SendAsync(request, cancellationToken);
		}
	}
}