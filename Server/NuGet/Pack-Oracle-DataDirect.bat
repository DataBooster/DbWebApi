@ECHO OFF
CD /D %~dp0
COPY Oracle\DataDirect\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.nuspec /Y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.csproj -IncludeReferencedProjects -Prop Configuration=Release;Platform=AnyCPU
MOVE /Y *.nupkg nupkg
