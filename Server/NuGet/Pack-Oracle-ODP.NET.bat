@ECHO OFF
CD /D %~dp0

IF NOT EXIST nupkg\Net45 MKDIR nupkg\Net45
COPY Oracle\ODP.NET\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Oracle.ODP.nuspec /Y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Oracle.ODP.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Net45

IF NOT EXIST nupkg\Net40 MKDIR nupkg\Net40
COPY Oracle\ODP.NET\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Net40.Oracle.ODP.nuspec /Y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Net40.Oracle.ODP.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Net40
