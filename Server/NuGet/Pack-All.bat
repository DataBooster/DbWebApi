@ECHO OFF
CD /D %~dp0

CALL Pack-SqlServer.bat

CALL Pack-Oracle-ODP.NET.Managed.bat

CALL Pack-Oracle-ODP.NET.bat

CALL Pack-Oracle-DataDirect.bat
