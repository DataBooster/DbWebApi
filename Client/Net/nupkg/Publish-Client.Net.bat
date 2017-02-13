@ECHO OFF
CD /D %~dp0

IF /i {%1}=={} GOTO :Usage
IF /i {%1}=={-h} GOTO :Usage
IF /i {%1}=={-help} GOTO :Usage

SET MERGE=..\..\..\packages\NupkgMerge.1.0.1.0\tools\NupkgMerge.exe
SET PUSH=..\..\..\.nuget\NuGet.exe
SET CNPKG=DataBooster.DbWebApi.Client.Net.%1

%MERGE% -p Net45\%CNPKG%.nupkg -s Net40\%CNPKG%.nupkg -o %CNPKG%.nupkg
%MERGE% -p Net45\%CNPKG%.symbols.nupkg -s Net40\%CNPKG%.symbols.nupkg -o %CNPKG%.symbols.nupkg

%PUSH% Push %CNPKG%.nupkg

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Publish-Client.Net.bat version
ECHO.
ECHO Example:
ECHO     Publish-Client.Net.bat 1.2.8.6
