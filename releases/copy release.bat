@echo off

set /P VERSION=What is the current library version (eg. v1.x)? 

mkdir %VERSION%
xcopy ..\src\SmartFormat\bin\Release\*.* %VERSION% /s

