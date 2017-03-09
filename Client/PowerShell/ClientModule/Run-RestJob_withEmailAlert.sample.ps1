# Run-RestJob_withEmailAlert.sample.ps1 - DbWebApi client PowerShell Example
# https://github.com/DataBooster/DbWebApi/tree/master/Client/PowerShell/ClientModule/
# Date: 2017-03-01

<#
	.SYNOPSIS
	Dispatches a DbWebApi job (in remote database) and sends an email notification for the job execution result.

	.DESCRIPTION
	Dispatches a DbWebApi job (or a method in a RESTful web service) and sends an email notification for the job execution result in the enterprise intranet.

	.PARAMETER Uri
	Specifies the Uniform Resource Identifier (URI) of the Internet resource to which the RESTful job to be run. This parameter is required.

	.PARAMETER InputJson
	The InputJson value will be passed in as-is to Invoke-RestMethod Body if it is a string, otherwise it will be treated as a object and converted to JSON string before passing to Invoke-RestMethod if ContentType is "application/json".

	.PARAMETER JobName
	Specifies a name for the job, this name will be the main part of the subject of the email notification.

	.PARAMETER EmailTo
	Specifies the addresses to which the email notification is sent. Enter names (optional) and the e-mail address, such as "Name <someone@example.com>". This parameter is required.

	.PARAMETER EmailFrom
	Specifies the address from which the email notification is sent. Enter a name (optional) and e-mail address, such as "Name <someone@example.com>". This parameter is required.

	.NOTES
	The variable $SmtpServers must be customized in this script or overridden in template config;
	The template of email body defined in "EmailTemplate.config.psm1" needs to be customized.

	.EXAMPLE
	Run-RestJob_withEmailAlert.ps1 -Uri 'http://dbwebapi.dev.com/sqldev/api/misc/whoami' -JobName 'Test Job1' -EmailTo 'someone@example.com' -EmailFrom 'someone@example.com'

	.LINK
	https://github.com/DataBooster/DbWebApi

	.LINK
	https://github.com/DataBooster/PS-WebApi
#>

[CmdletBinding()]
Param (
	[Parameter(Mandatory)]
	[Uri]$Uri,
	[Parameter()]
	$InputJson,
	[Parameter()]
	[string]$JobName,
	[Parameter(Mandatory)]
	[string[]]$EmailTo,
	[Parameter(Mandatory)]
	[string]$EmailFrom
)

#region TODO: Please update following $SmtpServers - list all SMTP servers can be used in your enterprise intranet.
New-Variable SmtpServers @("SMTP1.YOUR-COMPANY.COM", "SMTP2.YOUR-COMPANY.COM", "SMTP3.YOUR-COMPANY.COM") -Force;
#endregion

$Error.Clear();
Import-Module "Microsoft.PowerShell.Utility" -Cmdlet "Invoke-RestMethod";
Import-Module "$PSScriptRoot\DbWebApi-Client.psd1";
Import-Module "$PSScriptRoot\Send-Email.psd1";
Import-Module "$PSScriptRoot\EmailTemplate.config.psm1" -Scope Local -Force;
$StartTime = Get-Date;

#region main call
$Result = Invoke-DbWebApi -Uri $Uri -Body $InputJson -ErrorAction Continue;
#endregion

#region email notification
$Success = $Error.Count -eq 0;

If (!$JobName) {
	$JobName = $Uri.ToString();
}

$Subject = Get-EmailSubject -Success $Success -JobName $JobName;
if (![string]::IsNullOrWhiteSpace($Subject)) {
	$EmailBody = Get-EmailBody -Success $Success -ResultObject $Result -ElapsedTime ((Get-Date) - $StartTime);
	$null = Send-Email -To $EmailTo -From $EmailFrom -Subject $Subject -Body $EmailBody -SmtpServers $SmtpServers;
}
#endregion email notification

Exit $Error.Count;
