@echo off
title Backup
COLOR 2
SET BackupDir=%USERPROFILE%\Desktop\Poly-Backup
if not exist "%BackupDir%" mkdir %BackupDir%

cd %BackupDir%
if not exist "%USERPROFILE%\Documents\GitHub\Poly" ( 
	SET DIR="%USERPROFILE%\Dokumenter\GitHub\Poly" 
) ELSE (
	SET DIR="%USERPROFILE%\Documents\GitHub\Poly"
)

robocopy "%DIR%" "%BackupDir%" /mir /xf *.bin *.meta *.info /xd \ShaderCache\ \Library\