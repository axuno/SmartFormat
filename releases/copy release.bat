@echo off

ECHO Before continuing:
ECHO 1. Be sure to update the Version in "AssemblyInfo.cs".
ECHO 2. Be sure to create a "Release" build.
pause

set /P VERSION=What is the current library version (eg. v1.x)? 

mkdir %VERSION%
xcopy ..\src\SmartFormat\bin\Release\*.* %VERSION% /s

