if "%EMULATED%"=="true" goto :EOF

@echo off

REM Enable short names on volume C:
D:\Windows\System32\fsutil.exe 8dot3name set c: 0

REM Add a short name to the local storage folder
D:\Windows\System32\fsutil.exe file setshortname "C:\Resources\Directory\%RoleDeploymentID%.EVSWorker.CustomTempLocalStore" S

REM Add support for 32bit dlls
%windir%\system32\inetsrv\appcmd set config -section:applicationPools -applicationPoolDefaults.enable32BitAppOnWin64:true

EXIT /B 0