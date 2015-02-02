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
ExecuteDbApi is an extension method to ApiController provided by DbWebApi library.
``` CSharp
public static HttpResponseMessage ExecuteDbApi(this ApiController apiController,
                                               string sp, IDictionary<string, object> parameters)
// sp:         Specifies the fully qualified name of database stored procedure or function
// parameters: Specifies required parameters as name-value pairs
```


- Web.config  
"DataBooster.DbWebApi.MainConnection" is the only one configuration item needs to be customized:
``` Xml
	<connectionStrings>
		<add name="DataBooster.DbWebApi.MainConnection" providerName="System.Data.SqlClient" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SAMPLEDB;Integrated Security=SSPI" />
	</connectionStrings>
```

## NuGet
There are 4 NuGet packages for 4 differenct versions of ADO.NET providers:
- [DataBooster.DbWebApi.SqlServer - DbWebApi for SQL Server](http://www.nuget.org/packages/DataBooster.DbWebApi.SqlServer)
- [DataBooster.DbWebApi.Oracle.Managed - DbWebApi for Oracle (use ODP.NET Managed Driver)](http://www.nuget.org/packages/DataBooster.DbWebApi.Oracle.Managed)
- [DataBooster.DbWebApi.Oracle.ODP - DbWebApi for Oracle (use ODP.NET Provider)](http://www.nuget.org/packages/DataBooster.DbWebApi.Oracle.ODP)
- [DataBooster.DbWebApi.Oracle.DataDirect - DbWebApi for Oracle (use DataDirect Provider)](http://www.nuget.org/packages/DataBooster.DbWebApi.Oracle.DataDirect)

## Examples
