@ECHO OFF
REM Invoke-DbWebApi.bat - DbWebApi Client PowerShell Utility
REM https://github.com/databooster/dbwebapi
REM Date: 2016-01-20

PowerShell.exe -ExecutionPolicy Bypass -NonInteractive -File "%~dp0\Invoke-DbWebApi-Sample.ps1" %*
