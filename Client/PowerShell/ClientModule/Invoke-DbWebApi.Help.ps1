# Invoke-DbWebApi-Sample.ps1 - DbWebApi Client PowerShell Utility
# https://github.com/DataBooster/DbWebApi
# Date: 2016-03-03

Import-Module "Microsoft.PowerShell.Utility" -Cmdlet "Invoke-RestMethod"

Import-Module "$PSScriptRoot\DbWebApi-Client.psd1" -Force

Get-Help Invoke-DbWebApi @args
