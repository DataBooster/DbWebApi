# DbWebApi-Client-Sample.ps1 - DbWebApi Client PowerShell Utility
# https://github.com/databooster/dbwebapi
# Date: 2015-06-09

Import-Module $PSScriptRoot\DbWebApi-Client.psm1

Invoke-DbWebApi @args

Remove-Module DbWebApi-Client
