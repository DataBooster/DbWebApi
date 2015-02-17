# DbWebApi

### What is it?

DbWebApi is a .Net library that implement an entirely generic Web API for data-driven applications. It acts as a proxy service for web clients to call database (Oracle + SQL Server) stored procedures or functions out-of-box without any configuration or extra coding, the http response JSON or XML will have all Result Sets, Output Parameters and Return Value. If client request a CSV format (accept: text/csv), the http response will transmit the first result set as a CSV stream for large amounts of data. DbWebApi also supports xlsx (Excel 2007/2010) format response for multiple resultsets (each resultset presents as an Excel worksheet).

In other words, DbWebApi provides an alternative way to implement your Web APIs by implementing some stored procedures or functions in database. The DbWebApi will expose these stored procedures or functions as Web APIs straight away.

### What are the benefits of DbWebApi?

- In data-driven applications area, there are a large number of scenarios without substantial logic in data access web services, however they wasted a lot of our efforts on very boring data moving coding or configurations, we've had enough of it. Since now on, most of thus repetitive works can be dumped onto DbWebApi.
- DbWebApi can coexist within your existing ASP.NET Web API, as a supplementary service to reduce new boring manual works for most common of application scenarios. DbWebApi does not attempt to replace any existing methods or cover much specific application scenarios.

## Usage

#### ApiController:
``` CSharp
using System.Net.Http;
using System.Web.Http;
using System.Collections.Generic;
using DataBooster.DbWebApi;

namespace SampleDbWebApi.Controllers
{
    public class DbWebApiController : ApiController
    {
        [DbWebApiAuthorize]
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")]
        public HttpResponseMessage Execute(string sp, Dictionary<string, object> parameters)
        {
            return this.ExecuteDbApi(sp, parameters);
        }
    }
}
```
That's it!  
ExecuteDbApi is the extension method to ApiController.
``` CSharp
public static HttpResponseMessage ExecuteDbApi(this ApiController apiController,
                                               string sp, IDictionary<string, object> parameters)
// sp:         Specifies the fully qualified name of database stored procedure or function
// parameters: Specifies required input-parameters as name-value pairs
```

#### Web.config  
"DataBooster.DbWebApi.MainConnection" is the only one configuration item needs to be customized:
``` Xml
<connectionStrings>
  <add name="DataBooster.DbWebApi.MainConnection" providerName="System.Data.SqlClient" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SAMPLEDB;Integrated Security=SSPI" />
</connectionStrings>
```

#### Client Request  
##### Url:  
As registered in your WebApiConfig Routes (e.g. http://BaseUrl/Your.StoredProcedure.FullyQualifiedName)  
##### Input Parameters:  
Only required input-parameters of the stored procedure/function need to be specified in your request body as JSON format (Content-Type: application/json). Don't put parameter prefix ('@' or ':') in the JSON body.  
For example, a SQL Server Stored Procedure:  
``` SQL
ALTER PROCEDURE dbo.prj_GetRule
    @inRuleDate  datetime,
    @inRuleId    int,
    @inWeight    float(6) = 0.1,
    @outRuleDesc varchar(256) = NULL OUTPUT
AS  ...
```
The request JSON should like:  
``` JSON
{
    inRuleDate: "2015-02-03T00:00:00Z",
    inRuleId: 108
}
```
##### Accept Response MediaType:  
1. JSON (default)  
    Specify in request heade:  
    Accept: application/json  
    or  
    Accept: text/json  
    or specify in query string: ?format=json  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule?format=json)  
    or specify in UriPathExtension which depends on your url routing  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule/json)  
2. XML  
    Specify in request header:  
    Accept: application/xml  
    or  
    Accept: text/xml  
    or specify in query string: ?format=xml  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule?format=xml)  
    or specify in UriPathExtension which depends on your url routing  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule/xml)  
3. xlsx (Excel 2007/2010)  
    Specify in request header:  
    Accept: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet  
    or  
    Accept: application/ms-excel  
    or  
    Accept: application/xlsx  
    or specify in query string: ?format=xlsx  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule?format=xlsx)  
    or specify in UriPathExtension which depends on your url routing  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule/xlsx)  
4. CSV
    Specify in request header:  
    Accept: text/csv  
    or specify in query string: ?format=csv  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule?format=csv)  
    or specify in UriPathExtension which depends on your url routing  
       (e.g. http://BaseUrl/YourDatabase.dbo.prj_GetRule/csv)  
    Notes: current implementation CSV response will only return the first result set if your stored procedure has multiple result sets. It's considering to allow client to specify which result set to return in future releases.  
5. Other MediaTypes
    To support other MediaType, you can create a new class that implements the interface **IFormatPlug**, and register it in your HttpConfiguration. Just like following CSV and xlsx did:
``` CSharp
    public static void RegisterDbWebApi(this HttpConfiguration config)
    {
        config.AddFormatPlug(new CsvFormatPlug());
        config.AddFormatPlug(new XlsxFormatPlug());
    }
```

#### Response internal structure
``` CSharp
    public class StoredProcedureResponse
    {
        public List<List<BindableDynamicObject>> ResultSets { get; set; }
        public BindableDynamicObject OutputParameters { get; set; }
        public object ReturnValue { get; set; }
    }
```

#### Response body formats  
##### application/json, text/json  
    Sample:  
``` JSON
{
  "ResultSets":
  [
    [
      {"COL_1":"2015-02-03T00:00:00","COL_2":3.14159,"COL_3":"Hello World1","COL_4":null, "COL_5":0},
      {"COL_1":"2015-02-02T00:00:00","COL_2":3.14159,"COL_3":null,"COL_4":1234567.800099, "COL_5":1},
      {"COL_1":"2015-02-01T00:00:00","COL_2":3.14159,"COL_3":"Hello World3","COL_4":null, "COL_5":2},
      {"COL_1":"2015-01-31T00:00:00","COL_2":3.14159,"COL_3":null,"COL_4":9876541.230091, "COL_5":3}
    ],
    [
      {"COL_A":100,"COL_B":"fooA","COL_C":0},
      {"COL_A":200,"COL_B":"fooB","COL_C":null},
      {"COL_A":300,"COL_B":"fooC","COL_C":1}
    ],
    [
       {"NOTE":"Test1 for the third result set"},
       {"NOTE":"Test2 for the third result set"}
    ]
  ],
  "OutputParameters":
  {
    "outRuleDesc":"This is a test output parameter value.",
    "outSumTotal":888888.88,
    "outRC1":null
  },
  "ReturnValue":0
}
```

##### application/xml, text/xml  
    Sample:
``` XML
<StoredProcedureResponse xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/DbParallel.DataAccess">
  <OutputParameters>
    <outRuleDesc xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">This is a test output parameter value.</outRuleDesc>
    <outSumTotal xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:decimal" xmlns="">888888.88</outSumTotal>
    <outRC1 i:nil="true" xmlns="" />
  </OutputParameters>
  <ResultSets>
    <ArrayOfBindableDynamicObject>
      <BindableDynamicObject>
        <COL_1 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:dateTime" xmlns="">2015-02-03T00:00:00</COL_1>
        <COL_2 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:decimal" xmlns="">3.14159</COL_2>
        <COL_3 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">Hello World1</COL_3>
        <COL_4 i:nil="true" xmlns=""/>
        <COL_5 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">0</COL_5>
      </BindableDynamicObject>
      <BindableDynamicObject>
        <COL_1 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:dateTime" xmlns="">2015-02-02T00:00:00</COL_1>
        <COL_2 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:decimal" xmlns="">3.14159</COL_2>
        <COL_3 i:nil="true" xmlns=""/>
        <COL_4 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:decimal" xmlns="">1234567.800099</COL_4>
        <COL_5 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">1</COL_5>
      </BindableDynamicObject>
      <BindableDynamicObject>
        <COL_1 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:dateTime" xmlns="">2015-02-01T00:00:00</COL_1>
        <COL_2 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:decimal" xmlns="">3.14159</COL_2>
        <COL_3 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">Hello World3</COL_3>
        <COL_4 i:nil="true" xmlns=""/>
        <COL_5 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">2</COL_5>
      </BindableDynamicObject>
      <BindableDynamicObject>
        <COL_1 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:dateTime" xmlns="">2015-01-31T00:00:00</COL_1>
        <COL_2 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:decimal" xmlns="">3.14159</COL_2>
        <COL_3 i:nil="true" xmlns=""/>
        <COL_4 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:decimal" xmlns="">9876541.230091</COL_4>
        <COL_5 xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">3</COL_5>
      </BindableDynamicObject>
    </ArrayOfBindableDynamicObject>
    <ArrayOfBindableDynamicObject>
      <BindableDynamicObject>
        <COL_A xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">100</COL_A>
        <COL_B xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">fooA</COL_B>
        <COL_C xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">0</COL_C>
      </BindableDynamicObject>
      <BindableDynamicObject>
        <COL_A xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">200</COL_A>
        <COL_B xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">fooB</COL_B>
        <COL_C i:nil="true" xmlns=""/>
      </BindableDynamicObject>
      <BindableDynamicObject>
        <COL_A xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">300</COL_A>
        <COL_B xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">fooC</COL_B>
        <COL_C xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:int" xmlns="">1</COL_C>
      </BindableDynamicObject>
    </ArrayOfBindableDynamicObject>
    <ArrayOfBindableDynamicObject>
      <BindableDynamicObject>
        <NOTE xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">Test1 for the third result set</NOTE>
      </BindableDynamicObject>
      <BindableDynamicObject>
        <NOTE xmlns:d5p1="http://www.w3.org/2001/XMLSchema" i:type="d5p1:string" xmlns="">Test2 for the third result set</NOTE>
      </BindableDynamicObject>
    </ArrayOfBindableDynamicObject>
  </ResultSets>
  <ReturnValue i:nil="true" />
</StoredProcedureResponse>
```

##### text/csv  
    Sample:
``` CSV
COL_1,COL_2,COL_3,COL_4,COL_5
2015-02-03,3.14159,Hello World1,,0
2015-02-02,3.14159,,1234567.800099,1
2015-02-01,3.14159,Hello World3,,2
2015-01-31,3.14159,,9876541.230091,3
```

Notes:  
> JSON, XML and xlsx respones are constructed completely in Web API server before sending to the client, so you might  encounter OutOfMemoryException if the client wants to receive huge amounts of data. However, JSON can be sufficient in most application scenarios with its simplicity. And after all, process data as close to where the data physically resides as possible, this is a basic principle of big data processing. (i.e. Simplifying the complexity as early as possible.)  

>For most of Web applications, the final data are for human eyes to read.

>For some systems integration, CSV format is also widely used for data filling. It's mostly waste of human resources to design such SSIS packages one by one, and to maintain such encumbrances for ever. It's time for machine to do such mechanical process, let DbWebApi serve as the machine. No more mechanical designs, no more packages, no more configurations, no more deployments and no more maintenances. Let artificial complexities, dust to dust, nothing to nothing!

>CSV respone emerges as text stream pushing to the client, it just use very little memory in Web API server to push a few text lines as long as their CSV rows have been constructed, so on and so forth, until all complete. So the server's memory is not a limitation of how many records can be handled.

### Exceptions
For JSON, XML and xlsx responses, detail exception will be encapsulated into HttpResponseMessage with HTTP 500 error status if the Web API service encounters any problems. For the verbosity of errors to show in client side, it depends on your IncludeErrorDetailPolicy in HttpConfiguration. However, because CSV respone uses a push stream, the client side will always receive a HTTP 200 OK header without Content-Length field. If the server side encounter any exception subsequently, it would simply interrupt the http connection and the client would get a Receive Failure without any detail exception message.

### Permission Control
The example project shows using an authorization filter [DbWebApiAuthorize] to restrict which user can execute which stored procedure, that will integrate with your own implementation of permissions checking.
``` CSharp
    public class MyDbWebApiAuthorization : IDbWebApiAuthorization
    {
        public bool IsAuthorized(string userName, string storedProcedure)
        {
            // TO DO, to implementate your own authorization logic
            return true;	// If allow permission
            return false;	// If deny permission
        }
    }
```

## NuGet
There are 4 NuGet packages for 4 differenct versions of ADO.NET providers:
- [DbWebApi for SQL Server](http://www.nuget.org/packages/DataBooster.DbWebApi.SqlServer)
- [DbWebApi for Oracle (use ODP.NET Managed Driver)](http://www.nuget.org/packages/DataBooster.DbWebApi.Oracle.Managed)
- [DbWebApi for Oracle (use ODP.NET Provider)](http://www.nuget.org/packages/DataBooster.DbWebApi.Oracle.ODP)
- [DbWebApi for Oracle (use DataDirect Provider)](http://www.nuget.org/packages/DataBooster.DbWebApi.Oracle.DataDirect)

For-Oracle versions always contain the support for SQL Server. To switch from Oracle to SQL Server, simply change the providerName and connectionString of connectionStrings "DataBooster.DbWebApi.MainConnection" in your web.config.  
To switch above from one NuGet package to another NuGet Package, simply uninstall one and install another from NuGet Package Manager.

## Examples

Please refer to example project - MyDbWebApi in https://github.com/DataBooster/DbWebApi/tree/master/Examples/MyDbWebApi

The example project also requires Visual Studio 2010 or above with ASP.NET MVC 4 installed.
