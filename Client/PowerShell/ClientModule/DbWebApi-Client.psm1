<#
	.SYNOPSIS 
	Sends an HTTP or HTTPS request to a RESTful web service - simplified for DbWebApi invoking.

	.DESCRIPTION
	The Invoke-DbWebApi function wraps the Invoke-RestMethod cmdlet which sends HTTP and HTTPS requests to Representational State Transfer (REST) web services that returns richly structured data, and simplified the parameters preparation for DbWebApi invoking.

	Windows PowerShell formats the response based to the data type. For an RSS or ATOM feed, Windows PowerShell returns the Item or Entry XML nodes. For JavaScript Object Notation (JSON) or XML, Windows PowerShell converts (or deserializes) the content into objects.

	The Invoke-RestMethod cmdlet was introduced in Windows PowerShell 3.0.

	.COMPONENT
	Invoke-RestMethod

	.INPUTS
	System.Object. You can pipe the body of a web request to Invoke-DbWebApi. The $Body will be passed in as-is to Invoke-RestMethod if it is a string, otherwise it will be treated as a object and converted to JSON string before passing to Invoke-RestMethod if ContentType is "application/json".

	.PARAMETER Uri
	Specifies the Uniform Resource Identifier (URI) of the Internet resource to which the web request is sent. This parameter supports HTTP, HTTPS, FTP, and FILE values.

	(Inherited from Invoke-RestMethod)

	.PARAMETER Method
	Specifies the method used for the web api request. Defaults to Post.

	(Inherited from Invoke-RestMethod)

	.PARAMETER Body
	Enhanced: The Body value will be passed in as-is to Invoke-RestMethod if it is a string, otherwise it will be treated as a object and converted to JSON string before passing to Invoke-RestMethod if ContentType is "application/json".

	(Inherited from Invoke-RestMethod)

	.PARAMETER ContentType
	Specifies the content type of the web request. If this parameter is omitted, Invoke-DbWebApi sets the content type to "application/json".

	(Inherited from Invoke-RestMethod)

	.PARAMETER InFile
	Gets the content of the web request from a file.

	Enter a path and file name. If you omit the path, the default is the current location.

	(Inherited from Invoke-RestMethod)

	.PARAMETER OutFile
	Saves the response body in the specified output file. Enter a path and file name. If you omit the path, the default is the current location.

	By default, Invoke-DbWebApi returns the results to the pipeline. To send the results to a file and to the pipeline, use the Passthru parameter.

	(Inherited from Invoke-RestMethod)

	.PARAMETER PassThru
	Returns the results, in addition to writing them to a file. This parameter is valid only when the OutFile parameter is also used in the command.

	(Inherited from Invoke-RestMethod)

	.PARAMETER Credential
	Specifies a user account that has permission to send the request.

	(Inherited from Invoke-RestMethod)

	.PARAMETER BasicAuth
	Force to send the Basic Authorization header on the initial request if the Credential is presented. The plaintext username and password will be extracted from Credential parameter (PSCredential class).

	By default, Invoke-RestMethod will not send the Authorization header on the initial request. It waits for a challenge response then re-sends the request with the Authorization header.

	.PARAMETER Headers
	Specifies the headers of the web request. Enter a hash table or dictionary.

	(Inherited from Invoke-RestMethod)

	.PARAMETER Certificate
	Specifies the client certificate that is used for a secure web request. Enter a variable that contains a certificate or a command or expression that gets the certificate.

	(Inherited from Invoke-RestMethod)

	.PARAMETER CertificateThumbprint
	Specifies the digital public key certificate (X509) of a user account that has permission to send the request. Enter the certificate thumbprint of the certificate.

	Certificates are used in client certificate-based authentication. They can be mapped only to local user accounts; they do not work with domain accounts.

	(Inherited from Invoke-RestMethod)

	.PARAMETER DisableKeepAlive
	Sets the KeepAlive value in the HTTP header to False. By default, KeepAlive is True. KeepAlive establishes a persistent connection to the server to facilitate subsequent requests.

	(Inherited from Invoke-RestMethod)

	.PARAMETER TimeoutSec
	Specifies how long the request can be pending before it times out. Enter a value in seconds. The default value, 0, specifies an indefinite time-out.

	(Inherited from Invoke-RestMethod)

	.PARAMETER MaximumRedirection
	Determines how many times Windows PowerShell redirects a connection to an alternate Uniform Resource Identifier (URI) before the connection fails. The default value is 5. A value of 0 (zero) prevents all redirection.

	(Inherited from Invoke-RestMethod)

	.PARAMETER SessionVariable
	Creates a web request session and saves it in the value of the specified variable. Enter a variable name without the dollar sign ($) symbol. You cannot use the SessionVariable and WebSession parameters in the same command.

	(Inherited from Invoke-RestMethod)

	.PARAMETER TransferEncoding
	Specifies a value for the transfer-encoding HTTP response header. Valid values are Chunked, Compress, Deflate, GZip and Identity.

	(Inherited from Invoke-RestMethod)

	.PARAMETER UserAgent
	Specifies a user agent string for the web request.

	(Inherited from Invoke-RestMethod)

	.PARAMETER WebSession
	Specifies a web request session. Enter the variable name, including the dollar sign ($). You cannot use the SessionVariable and WebSession parameters in the same command.

	(Inherited from Invoke-RestMethod)

	.NOTES
	Since the Invoke-DbWebApi is just a simple wrap of the build-in Invoke-RestMethod cmdlet.
	Please see https://technet.microsoft.com/en-us/library/hh849971.aspx for more detail.

	.EXAMPLE
	$Response = Invoke-DbWebApi -Uri https://test-server:8089/something -Body @{inParamA = [DateTime]"2015-03-16"; inParamB = 108; inParamC = "Hello"}

	.LINK
	https://github.com/DataBooster/DbWebApi

	.LINK
	https://technet.microsoft.com/en-us/library/hh849971.aspx
#>

Function Invoke-DbWebApi {
	[CmdletBinding(SupportsShouldProcess)]
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
		[Parameter()]
		[switch]$PassThru,

		[Parameter()]
		[PSCredential]$Credential,
		[Parameter()]
		[switch]$BasicAuth,

		[Parameter()]
		[System.Collections.IDictionary]$Headers,
		[Parameter()]
		[System.Security.Cryptography.X509Certificates.X509Certificate]$Certificate,
		[Parameter()]
		[string]$CertificateThumbprint,
		[Parameter()]
		[switch]$DisableKeepAlive,
		[Parameter()]
		[int]$TimeoutSec,
		[Parameter()]
		[int]$MaximumRedirection,
		[Parameter()]
		[string]$SessionVariable,
		[Parameter()]
		[string]$TransferEncoding,
		[Parameter()]
		[string]$UserAgent,
		[Parameter()]
		[Microsoft.PowerShell.Commands.WebRequestSession]$WebSession
	)

	Begin
	{
		if ($Credential) {
			if ($BasicAuth) {
				$basicAuthHeader = "Basic " + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($($Credential.UserName) + ":" + $($Credential.GetNetworkCredential().Password)));
				if ($Headers) {
					$psBoundParameters.Headers["Authorization"] = $basicAuthHeader;
				}
				else {
					$psBoundParameters.Headers = @{ Authorization = $basicAuthHeader };
				}
				$psBoundParameters.Remove("BasicAuth") | Out-Null;
				$psBoundParameters.Remove("Credential") | Out-Null;
			}
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
