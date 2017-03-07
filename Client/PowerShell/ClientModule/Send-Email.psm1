<#
	.SYNOPSIS
	Sends an e-mail message through multiple SMTP servers (for redundant backup) in the enterprise intranet.

	.DESCRIPTION
	The Send-Email function wraps the build-in Send-MailMessage cmdlet to take advantage of multiple SMTP servers (for redundant backup) in the enterprise intranet.

	.COMPONENT
	Send-MailMessage

	.INPUTS
	System.String. You can pipe the path and file names of attachments to Send-Email.

	.PARAMETER From
	Specifies the address from which the mail is sent. Enter a name (optional) and e-mail address, such as "Name <someone@example.com>". This parameter is required.

	(Inherited from Send-MailMessage)

	.PARAMETER To
	Specifies the addresses to which the mail is sent. Enter names (optional) and the e-mail address, such as "Name <someone@example.com>". This parameter is required.

	(Inherited from Send-MailMessage)

	.PARAMETER Cc
	Specifies the e-mail addresses to which a carbon copy (CC) of the e-mail message is sent. Enter names (optional) and the e-mail address, such as "Name <someone@example.com>".

	(Inherited from Send-MailMessage)

	.PARAMETER Priority
	Specifies the priority of the e-mail message. The valid values for this are Normal, High, and Low. Normal is the default.

	(Inherited from Send-MailMessage)

	.PARAMETER Subject
	Specifies the subject of the e-mail message. This parameter is required.

	(Inherited from Send-MailMessage)

	.PARAMETER Body
	Specifies the body (content) of the e-mail message.

	(Inherited from Send-MailMessage)

	.PARAMETER Attachments
	Specifies the path and file names of files to be attached to the e-mail message. You can use this parameter or pipe the paths and file names to Send-Email.

	(Inherited from Send-MailMessage)

	.PARAMETER SmtpServers
	Specifies multiple SMTP servers, the name of the first available SMTP server will be stored in the global variable $PSEmailServer.

	If a value of the $PSEmailServer preference variable already exists, this parameter will be ignored.

	If the preference variable $PSEmailServer is never set and this parameter is omitted, the command fails.

	.NOTES
	Since the Send-Email is just a simple wrap of the build-in Send-MailMessage cmdlet.
	Please see https://msdn.microsoft.com/en-us/powershell/reference/5.1/microsoft.powershell.utility/Send-MailMessage for more detail.

	.EXAMPLE
	Send-Email -From 'someone@example.com' -To 'someone@example.com' -Subject 'the test job' -Body '<html><body>...</body></html>' -SmtpServers 'smtp1.your-company.com",'smtp2.your-company.com','smtp3.your-company.com'

	.LINK
	https://github.com/DataBooster/DbWebApi

	.LINK
	https://github.com/DataBooster/PS-WebApi

	.LINK
	https://msdn.microsoft.com/en-us/powershell/reference/5.1/microsoft.powershell.utility/Send-MailMessage
#>

function Send-Email {
	[CmdletBinding(SupportsShouldProcess)]
	param(
		[Parameter(Mandatory)]
		[string]$From,
		[Parameter(Mandatory)]
		[string[]]$To,
		[Parameter()]
		[string[]]$Cc,
		[Parameter()]
		[System.Net.Mail.MailPriority]$Priority,
		[Parameter(Mandatory, ValueFromPipelineByPropertyName)]
		[string]$Subject,
		[Parameter(ValueFromPipelineByPropertyName)]
		[string]$Body,
		[Parameter(ValueFromPipeline)]
		[string[]]$Attachments,
        [Parameter()][ValidateCount(1,999)]
        [string[]]$SmtpServers
	)
    begin {
        $PSBoundParameters.Remove("SmtpServers");
        $PSBoundParameters.Encoding = [System.Text.UTF8Encoding]::UTF8;
    }

	process {
		$BodyAsHtml = IsBodyHtml -EmailBody $Body;

        if (!$global:PSEmailServer -and $SmtpServers) {
    		foreach ($es in $SmtpServers) {
                $global:PSEmailServer = $es;
		    	try	{
			    	return Send-MailMessage @PSBoundParameters -BodyAsHtml:$BodyAsHtml -ErrorAction Stop;
			    }
			    catch [System.Net.Mail.SmtpException] { }
			}
        }

        return Send-MailMessage @PSBoundParameters -BodyAsHtml:$BodyAsHtml;
	}
}

function IsBodyHtml {
    param(
		[Parameter(Mandatory)]
		[string]$EmailBody
    )

	if ($EmailBody -and $EmailBody.Length -ge 13) {
		$body = $EmailBody.ToLower();
		$htmlBegin = $body.IndexOf("<html");
		if ($htmlBegin -ge 0 -and $htmlBegin + 13 -le $body.Length) {
			$htmlEnd = $body.LastIndexOf("</html>");
			return $htmlBegin -lt $htmlEnd;
		}
	}

	return $false;
}
