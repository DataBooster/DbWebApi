@ECHO OFF
REM Invoke-DbWebApi.bat - DbWebApi Client PowerShell Utility
REM https://github.com/DataBooster/DbWebApi
REM Date: 2016-01-20

PowerShell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "%~dp0\Invoke-DbWebApi-Sample.ps1" %*

REM EXIT /B %ERRORLEVEL%
