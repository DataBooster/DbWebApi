@ECHO OFF
CD /D %~dp0

IF NOT EXIST Net45 MKDIR Net45
..\..\..\.nuget\NuGet.exe pack ..\WebApi-Client\WebApi-Client.csproj -Tool -Symbols -Prop Configuration=Release;Platform=AnyCPU -OutputDirectory Net45
