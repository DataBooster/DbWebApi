@ECHO OFF
CD /D %~dp0

IF /i {%1}=={} GOTO :Usage
IF /i {%1}=={-h} GOTO :Usage
IF /i {%1}=={-help} GOTO :Usage

CALL Merge-All.bat %1
CALL Push-All.bat %1

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Publish-All.bat version
ECHO.
ECHO Example:
ECHO     Publish-All.bat 1.2.8.6
