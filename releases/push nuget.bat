@echo off

ECHO Creating package...

set NUGET=NuGet.exe
set TARGETDIR=.

DEL %TARGETDIR%\*.nupkg

%NUGET% pack -Verbosity detailed -OutputDirectory %TARGETDIR% ..\src\SmartFormat\SmartFormat.csproj


ECHO Pushing to NuGet...

set /P APIKEY=Enter your Api Key from https://www.nuget.org/account: 

if "%APIKEY%"=="" GOTO ERROR

%NUGET% push %TARGETDIR%\*.nupkg %APIKEY%
GOTO END

:ERROR
echo Canceled: No api key was entered.

:END