# Invoke-DbWebApi-Sample.ps1 - DbWebApi Client PowerShell Utility
# https://github.com/DataBooster/DbWebApi
# Date: 2015-06-09

Import-Module "Microsoft.PowerShell.Utility"
Import-Module "$PSScriptRoot\DbWebApi-Client.psm1"

Invoke-DbWebApi @args

# Remove-Module DbWebApi-Client

Exit $error.Count;
