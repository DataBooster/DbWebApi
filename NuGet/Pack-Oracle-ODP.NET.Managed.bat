@ECHO OFF
CD /D %~dp0
COPY Oracle\ODP.NET.Managed\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.nuspec /Y
..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.csproj -IncludeReferencedProjects -Prop Configuration=Release;Platform=AnyCPU
MOVE /Y *.nupkg nupkg
