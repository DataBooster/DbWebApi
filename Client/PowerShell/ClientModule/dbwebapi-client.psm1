<#
	Invoke-DbWebApi

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

	begin
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

	process
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
