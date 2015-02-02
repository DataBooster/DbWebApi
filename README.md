# DbWebApi

### What is it?

DbWebApi is a .Net library that implement an entirely generic Web API for data-driven applications. It acts as a proxy service for web clients to call database (Oracle + SQL Server) stored procedures or functions out-of-box without any configuration or extra coding, the http response JSON or XML will have all Result Sets, Output Parameters and Return Value. If client request a CSV format (accept: text/csv), the http response will transmit the first result set as a CSV stream for almost unlimited number of rows.

### What are the benefits of DbWebApi?

- In data-driven applications area, there are a large number of scenarios without substantial logic in data access web services, however they wasted a lot of our efforts on very boring data moving coding or configurations, we've had enough of it. Since now on, most of thus repetitive works should be dumped onto DbWebApi.
- DbWebApi can coexist within your existing ASP.NET Web API, as a supplementary service to reduce new boring manual works for most common of application scenarios. DbWebApi does not attempt to replace any existing methods or cover much specific application scenarios.

## Usage

- ApiController:
``` CSharp
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using DataBooster.DbWebApi;

namespace SampleDbWebApi.Controllers
{
	public class DbWebApiController : ApiController
	{
			[HttpGet]
			[HttpPost]
			[HttpPut]
			[HttpDelete]
			[CustomPermissionFilter]
			public HttpResponseMessage Execute(string sp, IDictionary<string, object> parameters)
			{
				return this.ExecuteDbApi(sp, parameters);
			}
	}
}
```
That's it!
ExecuteDbApi is an extension method provided by DbWebApi library.
``` CSharp
public static HttpResponseMessage ExecuteDbApi(this ApiController apiController,
                                               string sp, IDictionary<string, object> parameters)
// sp:         Specifies the fully qualified name of database stored procedures or functions
// parameters: Specifies required parameters as name-value pairs
```

- Web.config
	<connectionStrings>
		<add name="DataBooster.DbWebApi.MainConnection" providerName="System.Data.SqlClient" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SAMPLEDB;Integrated Security=SSPI" />
	</connectionStrings>
