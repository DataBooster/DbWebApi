<#
	.SYNOPSIS 
	Sends an HTTP or HTTPS request to a RESTful web service - simplified for DbWebApi invoking.

	.DESCRIPTION
	The Invoke-DbWebApi function wraps the Invoke-RestMethod cmdlet which sends HTTP and HTTPS requests to Representational State Transfer (REST) web services
	that returns richly structured data, and simplified the parameters preparation for DbWebApi invoking.

	Windows PowerShell formats the response based to the data type. For an RSS or ATOM feed, Windows PowerShell returns the Item or Entry XML 
	nodes. For JavaScript Object Notation (JSON) or XML, Windows PowerShell converts (or deserializes) the content into objects.

	The Invoke-RestMethod cmdlet was introduced in Windows PowerShell 3.0.

	.COMPONENT
	Invoke-RestMethod

	.INPUTS
	System.Object. You can pipe the body of a web request to Invoke-DbWebApi. The $Body will be passed in as-is to Invoke-RestMethod if it is a string, 
	otherwise it will be treated as a object and converted to JSON string before passing to Invoke-RestMethod if ContentType is "application/json".

	.PARAMETER Body
	Enhanced: The Body value will be passed in as-is to Invoke-RestMethod if it is a string, otherwise it will be treated as a object and converted to 
	JSON string before passing to Invoke-RestMethod if ContentType is "application/json".

	.PARAMETER Uri
	Specifies the Uniform Resource Identifier (URI) of the Internet resource to which the web request is sent. This parameter supports HTTP, 
	HTTPS, FTP, and FILE values.

	.PARAMETER Method
	Specifies the method used for the web api request. Defaults to Post.

	.PARAMETER ContentType
	Specifies the content type of the web request. If this parameter is omitted, Invoke-DbWebApi sets the content type to "application/json".

	.PARAMETER InFile
	Gets the content of the web request from a file.

	Enter a path and file name. If you omit the path, the default is the current location.

	.PARAMETER OutFile
	Saves the response body in the specified output file. Enter a path and file name. If you omit the path, the default is the current location.

	By default, Invoke-DbWebApi returns the results to the pipeline. To send the results to a file and to the pipeline, use the Passthru parameter.

	.PARAMETER User
	Specifies a user name if you want to use Basic Authorization to send the request.
	Otherwise Invoke-DbWebApi always uses the credentials of the current user to send the web request. -UseDefaultCredentials

	.PARAMETER Password
	Specifies the password of the specified user name, once you use Basic Authorization to send the request.

	.NOTES
	Any other valid parameters of Invoke-RestMethod can also be applied to the Invoke-DbWebApi, since it is just a simple wrap of build-in Invoke-RestMethod cmdlet.
	For detail about all valid parameters, please see https://technet.microsoft.com/en-us/library/hh849971.aspx.

	.EXAMPLE
	$Response = Invoke-DbWebApi -Uri https://test-server:8089/something -Body @{inParamA = [DateTime]"2015-03-16"; inParamB = 108; inParamC = "Hello"}
	(Using Integrated Windows Authentication.)

	.EXAMPLE
	Invoke-DbWebApi -Uri https://test-server:8089/somethingelse/csv -Method Get -User guest108 -Password "Hello World!" -OutFile "C:\Temp\test.csv"
	(Using Basic Authentication.)

	.LINK
	https://github.com/DataBooster/DbWebApi

	.LINK
	https://technet.microsoft.com/en-us/library/hh849971.aspx
#>

Function Invoke-DbWebApi {
	[CmdletBinding(DefaultParameterSetName = 'DefaultAuth', SupportsShouldProcess)]
	Param (
		[Parameter(Mandatory)]
		[Uri]$Uri,
		[Parameter()]
		[Microsoft.PowerShell.Commands.WebRequestMethod]$Method = [Microsoft.PowerShell.Commands.WebRequestMethod]::Post,
		[Parameter(ValueFromPipeline)]
		$Body,
		[Parameter()]
		[string]$ContentType = "application/json",
		[Parameter()]
		[string]$InFile,
		[Parameter()]
		[string]$OutFile,
		[Parameter(Mandatory, ParameterSetName = 'BasicAuth')]
		[ValidateNotNullOrEmpty()]
		[string]$User,
		[Parameter(Mandatory, ParameterSetName = 'BasicAuth')]
		[string]$Password
	)

	Begin
	{
		if ($PSCmdlet.ParameterSetName -eq 'BasicAuth') {
			$basicAuth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($($User) + ":" + $($Password)));
			$psBoundParameters.Headers = @{ Authorization = $basicAuth };
			$psBoundParameters.Remove("User") | Out-Null;
			$psBoundParameters.Remove("Password") | Out-Null;
		}
		else {
			$psBoundParameters.UseDefaultCredentials = $true;
		}

		if (!$psBoundParameters.ContainsKey("Method")) {
			$psBoundParameters.Method = $Method;
		}

		if (!$psBoundParameters.ContainsKey("ContentType")) {
			$psBoundParameters.ContentType = $ContentType;
		}
	}

	Process
	{
		if ($Body -and $ContentType -eq "application/json" -and $Body -isnot [string]) {
			$psBoundParameters.Body = ConvertTo-Json $Body;
		}

		if ($PSCmdlet.ShouldProcess("Invoke-RestMethod -Uri '$Uri' ...")) {
			Invoke-RestMethod @psBoundParameters;
		}
		else {
			Write-Verbose (ConvertTo-Json $psBoundParameters);
		}
	}
}
