@echo off
rem Helper script for those who want to run psake from cmd.exe
rem Example run from cmd.exe:
rem psake "BuildHelloWord" "4.0" 

if '%1'=='/?' goto help
if '%1'=='-help' goto help
if '%1'=='-h' goto help

powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0\var\psake\psake.ps1' Bumbershoot.build.ps1 %*; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%

:help
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0\var\psake\psake.ps1' -help"
