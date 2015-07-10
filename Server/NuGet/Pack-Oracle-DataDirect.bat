@ECHO OFF
CD /D %~dp0

COPY Oracle\DataDirect\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.nuspec /Y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Net45

COPY Oracle\DataDirect\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Net40.nuspec /Y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Net40.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Net40
