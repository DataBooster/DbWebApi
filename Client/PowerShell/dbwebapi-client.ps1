# DbWebApi Client PowerShell Utility Library v1.0.1-alpha
# https://github.com/databooster/dbwebapi
# Date: 2015-06-09

Function Invoke-DbWebApi {
	Param (
		[Parameter(Mandatory=$true)]
		[Uri]$Uri,
		[Parameter(Mandatory=$false)]
		[Microsoft.PowerShell.Commands.WebRequestMethod]$Method = [Microsoft.PowerShell.Commands.WebRequestMethod]::Post,
		[Parameter(Mandatory=$false)]
		$Body,
		[Parameter(Mandatory=$false)]
		[string]$ContentType = "application/json",
		[Parameter(Mandatory=$false)]
		[string]$InFile,
		[Parameter(Mandatory=$false)]
		[string]$OutFile,
		[Parameter(Mandatory=$false)]
		[string]$User,
		[Parameter(Mandatory=$false)]
		[string]$Password
	);

	if ($User) {
		$basicAuth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($($User) + ":" + $($Password)));
		$psBoundParameters.Headers = @{ Authorization = $basicAuth };
		$psBoundParameters.Remove("User");
		$psBoundParameters.Remove("Password");
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

	if ($Body -and $ContentType -eq "application/json") {
		$psBoundParameters.Body = ConvertTo-Json $Body;
	}

	Invoke-RestMethod @psBoundParameters;
}
