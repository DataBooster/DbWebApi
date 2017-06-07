@ECHO OFF
CD /D %~dp0

IF /i {%1}=={} GOTO :Usage
IF /i {%1}=={-h} GOTO :Usage
IF /i {%1}=={-help} GOTO :Usage

SET MERGE=..\..\..\packages\NupkgMerge.1.0.1.0\tools\NupkgMerge.exe
SET PUSH=..\..\..\.nuget\NuGet.exe
SET CUPKG=Net45\DataBooster.DbWebApi.Client.CmdUtility.%1

%PUSH% Push %CUPKG%.nupkg

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Publish-CmdUtility.bat version
ECHO.
ECHO Example:
ECHO     Publish-CmdUtility.bat 1.0.0.0
