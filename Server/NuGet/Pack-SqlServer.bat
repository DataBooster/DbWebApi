@ECHO OFF
CD /D %~dp0

COPY SqlServer\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.nuspec /y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Net45

COPY SqlServer\DataBooster.DbWebApi.nuspec ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Net40.nuspec /y
..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi\DataBooster.DbWebApi.Net40.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\Net40
