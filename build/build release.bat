@echo off

set PWD="%CD%"
set NUGET=%PWD%\NuGet.exe

DEL %PWD%\..\*.nupkg

%NUGET% pack -Verbosity detailed -OutputDirectory %PWD%\.. %PWD%\..\SmartFormat.NET.nuspec