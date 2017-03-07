$Global:SmtpServers = @("SMTP1.MY-COMPANY.COM", "SMTP2.MY-COMPANY.COM", "SMTP3.MY-COMPANY.COM");

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
        return @"
The job has completed successfully.

Additional information ...

Elapsed: $ElapsedTime
"@;
    } else {
        $ErrorDetail = [string]::Join("`r`n", ($Global:Error | ForEach {$_.ErrorDetails.Message + "`r`n" + $_.Exception.Message}));

        return @"
The job has failed.

Additional information ...

Elapsed: $ElapsedTime
ErrorDetails:
$ErrorDetail
"@;
    }
}
