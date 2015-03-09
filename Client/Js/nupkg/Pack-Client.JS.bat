@ECHO OFF
CD /D %~dp0
..\..\..\.nuget\NuGet.exe pack DataBooster.DbWebApi.Client.JS.nuspec
