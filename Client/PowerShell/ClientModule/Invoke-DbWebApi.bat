@ECHO OFF
REM Invoke-DbWebApi.bat - DbWebApi Client PowerShell Utility
REM https://github.com/DataBooster/DbWebApi
REM Date: 2016-01-20

IF "%1"=="" GOTO :Usage
IF "%1"=="?" GOTO :Usage
IF "%1"=="/?" GOTO :Usage
IF "%1"=="-?" GOTO :Usage
FOR %%A IN ("-h","-help","/h","/help","help") DO IF /i "%1"==%%A GOTO :Usage

PowerShell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "%~dp0\Invoke-DbWebApi.Sample.ps1" %*

EXIT /B %ERRORLEVEL%

:Usage

IF "%2"=="" (
PowerShell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0\Invoke-DbWebApi.Help.ps1" -Detailed
) ELSE (
PowerShell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0\Invoke-DbWebApi.Help.ps1" %2 %3 %4 %5 %6 %7 %8 %9
)
