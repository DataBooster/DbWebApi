@ECHO OFF
CD /D %~dp0

IF /i {%1} == {} GOTO :Usage
IF /i {%1} == {-h} GOTO :Usage
IF /i {%1} == {-help} GOTO :Usage

SET PKGSQL=nupkg\DataBooster.DbWebApi.SqlServer.%1.nupkg
SET PKGOMG=nupkg\DataBooster.DbWebApi.Oracle.Managed.%1.nupkg
SET PKGODP=nupkg\DataBooster.DbWebApi.Oracle.ODP.%1.nupkg
SET PKGDDT=nupkg\DataBooster.DbWebApi.Oracle.DataDirect.%1.nupkg
SET PKGPREPARED=true

FOR %%P IN (%PKGSQL% %PKGOMG% %PKGODP% %PKGDDT%) DO (
IF NOT EXIST %%P (
ECHO %%P does not exist!
SET PKGPREPARED=false
)
)

IF {%PKGPREPARED%} == {false} (
COLOR 0C
PAUSE
COLOR
GOTO :EOF
)

FOR %%P IN (%PKGSQL% %PKGOMG% %PKGODP% %PKGDDT%) DO ..\..\.nuget\NuGet.exe Push %%P

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Push-All.bat version
ECHO.
ECHO Example:
ECHO     Push-All.bat 1.2.7.6
