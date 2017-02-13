@ECHO OFF
CD /D %~dp0

IF /i {%1}=={} GOTO :Usage
IF /i {%1}=={-h} GOTO :Usage
IF /i {%1}=={-help} GOTO :Usage

SET MERGE=..\..\packages\NupkgMerge.1.0.1.0\tools\NupkgMerge.exe
SET PKGSQL=SqlServer.%1
SET PKGOMG=Oracle.Managed.%1
SET PKGODP=Oracle.ODP.%1
SET PKGDDT=Oracle.DataDirect.%1
SET PKGPREPARED=true

FOR %%P IN (%PKGSQL% %PKGOMG% %PKGODP% %PKGDDT%) DO (
IF NOT EXIST nupkg\Net45\DataBooster.DbWebApi.%%P.nupkg (
ECHO 'nupkg\Net45\DataBooster.DbWebApi.%%P.nupkg' does not exist!
SET PKGPREPARED=false
)
IF NOT EXIST nupkg\Net45\DataBooster.DbWebApi.%%P.symbols.nupkg (
ECHO 'nupkg\Net45\DataBooster.DbWebApi.%%P.symbols.nupkg' does not exist!
SET PKGPREPARED=false
)
IF NOT EXIST nupkg\Net40\DataBooster.DbWebApi.%%P.nupkg (
ECHO 'nupkg\Net40\DataBooster.DbWebApi.%%P.nupkg' does not exist!
SET PKGPREPARED=false
)
IF NOT EXIST nupkg\Net40\DataBooster.DbWebApi.%%P.symbols.nupkg (
ECHO 'nupkg\Net40\DataBooster.DbWebApi.%%P.symbols.nupkg' does not exist!
SET PKGPREPARED=false
)
)

IF {%PKGPREPARED%}=={false} (
COLOR 0C
PAUSE
COLOR
GOTO :EOF
)

FOR %%P IN (%PKGSQL% %PKGOMG% %PKGODP% %PKGDDT%) DO %MERGE% -p nupkg\Net45\DataBooster.DbWebApi.%%P.nupkg -s nupkg\Net40\DataBooster.DbWebApi.%%P.nupkg -o nupkg\DataBooster.DbWebApi.%%P.nupkg
FOR %%P IN (%PKGSQL% %PKGOMG% %PKGODP% %PKGDDT%) DO %MERGE% -p nupkg\Net45\DataBooster.DbWebApi.%%P.symbols.nupkg -s nupkg\Net40\DataBooster.DbWebApi.%%P.symbols.nupkg -o nupkg\DataBooster.DbWebApi.%%P.symbols.nupkg

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Merge-All.bat version
ECHO.
ECHO Example:
ECHO     Merge-All.bat 1.2.8.6
