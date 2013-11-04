@echo off
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Copyright 2012-2013 Rare Crowds, Inc.
::
::   Licensed under the Apache License, Version 2.0 (the "License");
::   you may not use this file except in compliance with the License.
::   You may obtain a copy of the License at
::
::       http://www.apache.org/licenses/LICENSE-2.0
::
::   Unless required by applicable law or agreed to in writing, software
::   distributed under the License is distributed on an "AS IS" BASIS,
::   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
::   See the License for the specific language governing permissions and
::   limitations under the License.
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

setlocal enabledelayedexpansion
pushd %~dp0

if '%Target%'=='' set Target=ApnxAppSand
set Configs=%TEMP%\%Random%
dir /b/s *.Local.* >%Configs%

:: Replace configs for Local TargetPlatform with ApnxAppSand
for /f %%i in ('type %Configs%') do call :Replace %%i
echo Replaced Local configs with %Target%

:: Hack etc/hosts to map rcprotoapnxapp.cloudapp.net to localhost
set ETC=%SystemRoot%\System32\Drivers\etc
echo f | xcopy /yrq %ETC%\hosts %ETC%\~hosts.bak >NUL
echo 127.0.0.1 rcprotoapnxapp.cloudapp.net >> %ETC%\hosts
echo ::1 rcprotoapnxapp.cloudapp.net >> %ETC%\hosts
echo Added localhost mapping for rcprotoapnxapp.cloudapp.net

echo.
echo Configured for local run of %Target%.
echo Press any key to revert...
pause >NUL
echo.

:: Restore Local configs
for /f %%i in ('type %Configs%') do call :Restore %%i
echo Restored Local configs

:: Revert etc/hosts hack
echo f | xcopy /yr %ETC%\~hosts.bak %ETC%\hosts
del /f %ETC%\~hosts.bak
del /f %Configs%

popd
endlocal
pause
goto :EOF

:Replace
set LocalCfg=%1
set TargetCfg=%LocalCfg:Local=!Target!%
set Backup=%LocalCfg%.bak
if exist %TargetCfg% (
  echo f | xcopy /yr %LocalCfg% %Backup% >NUL
  xcopy /yr %TargetCfg% %LocalCfg% >NUL
)
goto :EOF

:Restore
set LocalCfg=%1
set TargetCfg=%LocalCfg:Local=!Target!%
set Backup=%LocalCfg%.bak
if exist %Backup% (
  echo f | xcopy /yr %Backup% %LocalCfg% >NUL
  del %Backup%
)
goto :EOF
