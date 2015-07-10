@ECHO OFF
CD /D %~dp0

COPY ..\DataBooster.DbWebApi.Client\DataBooster.DbWebApi.Client.nuspec ..\DataBooster.DbWebApi.Client\DataBooster.DbWebApi.Client.Net40.nuspec /Y

IF NOT EXIST Net45 MKDIR Net45
..\..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi.Client\DataBooster.DbWebApi.Client.csproj -IncludeReferencedProjects -Symbols -Prop Configuration=Release;Platform=AnyCPU -OutputDirectory Net45

IF NOT EXIST Net40 MKDIR Net40
..\..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi.Client\DataBooster.DbWebApi.Client.Net40.csproj -IncludeReferencedProjects -Symbols -Prop Configuration=Release;Platform=AnyCPU -OutputDirectory Net40
