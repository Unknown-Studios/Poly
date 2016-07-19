@echo off
Set Unity="D:\Programs\Editor\Unity.exe"
if exist %Unity% ( goto Build )
Set Unity="C:\Program Files (x86)\Unity\Editor\Unity.exe"
if exist %Unity% ( goto Build )
Set Unity="C:\Program Files\Unity\Editor\Unity.exe"
if exist %Unity% ( goto Build )
	

:Build:
echo Building Windows Standalone..
%Unity% -quit -batchmode -buildWindowsPlayer "%USERPROFILE%\Dropbox\Poly Game Files\Poly.exe"
echo Windows Build Ready
echo -------------------------
echo Building Mac Standalone..
%Unity% -quit -batchmode -buildOSXPlayer  "%USERPROFILE%\Dropbox\Poly Game Files\Poly.app"
echo Mac Build Ready