@ECHO OFF
CD /D %~dp0
..\..\..\.nuget\NuGet.exe pack ..\DataBooster.DbWebApi.Client\DataBooster.DbWebApi.Client.csproj -IncludeReferencedProjects -Prop Configuration=Release;Platform=AnyCPU
