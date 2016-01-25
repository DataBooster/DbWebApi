# Invoke-DbWebApi-Sample.ps1 - DbWebApi Client PowerShell Utility
# https://github.com/DataBooster/DbWebApi
# Date: 2015-06-09

Import-Module "Microsoft.PowerShell.Utility" -Cmdlet "Invoke-RestMethod"

Import-Module "$PSScriptRoot\DbWebApi-Client.psd1" -Force

Invoke-DbWebApi @args

Exit $error.Count;
