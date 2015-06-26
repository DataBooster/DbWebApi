@ECHO OFF
CD /D %~dp0
COPY SqlServer\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.nuspec /y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.csproj -IncludeReferencedProjects -Prop Configuration=Release;Platform=AnyCPU
MOVE /Y *.nupkg nupkg
