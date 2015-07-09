@ECHO OFF
CD /D %~dp0
COPY Oracle\ODP.NET\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.nuspec /Y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.csproj -IncludeReferencedProjects -Symbols -Prop Configuration=Release;Platform=AnyCPU
MOVE /Y *.nupkg nupkg
