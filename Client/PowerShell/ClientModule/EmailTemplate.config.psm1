#region TODO: You can override the value of $SmtpServers which initially defined in the caller script (Run-RestJob_withEmailAlert.sample.ps1)
$SmtpServers = @("SMTP1.MY-COMPANY.COM", "SMTP2.MY-COMPANY.COM", "SMTP3.MY-COMPANY.COM");
#endregion

function Get-EmailSubject {
	param (
		[Parameter(Mandatory)]
		[bool]$Success,
		[Parameter(Mandatory)]
		[string]$JobName
	)
	if ($Success) {
		return "COMPLETED: " + $JobName + " has completed successfully";
	} else {
		return "FAILED: " + $JobName + " has failed";
	}
}

function Get-EmailBody {
	param (
		[Parameter(Mandatory)]
		[bool]$Success,
		[Parameter()]
		$ResultObject,
		[Parameter()]
		[TimeSpan]$ElapsedTime
	)

	if ($Success) {

#region TODO: The template of email body for success case
		return @"
The job has completed successfully.

Additional information ...

Elapsed: $ElapsedTime
"@;
#endregion

	} else {
		$ErrorDetail = [string]::Join("`r`n", ($Global:Error | ForEach {$_.ErrorDetails.Message + "`r`n" + $_.Exception.Message}));

#region TODO: The template of email body for failure case
		return @"
The job has failed.

Additional information ...

Elapsed: $ElapsedTime
ErrorDetails:
$ErrorDetail
"@;
#endregion

	}
}

Export-ModuleMember -Function Get-Email* -Variable SmtpServers;
