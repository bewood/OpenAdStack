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

::
:: Locates the PrivateConfig for the current environment
::

set PrefsDir=%USERPROFILE%\OpenAdStack
set EnvRoot=%~dp0..

:: Clear OpenAdStack.PrivateConfig if it is currently set to the setup template
for /f "delims=" %%i in ('echo %OpenAdStack.PrivateConfig% ^| find /i "build\Setup\PrivateConfig"') do (
  set OpenAdStack.PrivateConfig=
)

set PrivateConfigSearchPath=^
%OpenAdStack.PrivateConfig%;^
%EnvRoot%\..\..\LucyConfig;^
%PrefsDir%\PrivateConfig;^
%EnvRoot%\PrivateConfig;^
%EnvRoot%\..\PrivateConfig;^
%EnvRoot%\..\..\PrivateConfig;^
%EnvRoot%\build\Setup\PrivateConfig

for /d %%i in (!PrivateConfigSearchPath!) do (
  set PRIVATECONFIG=%%~dpni
  if exist "!PRIVATECONFIG!" (
    goto FoundPrivateConfig
  )
)
goto Failed

:FoundPrivateConfig
:: Set OpenAdStack.PrivateConfig user environment variable
:: This is used to build in the Visual Studio IDE
if not '%OpenAdStack.PrivateConfig%'=='%PRIVATECONFIG%' (
  echo Updating OpenAdStack.PrivateConfig...
  call %~dp0\SetEnvVarHKCU.cmd "OpenAdStack.PrivateConfig" "%PRIVATECONFIG:\=\\%"
)
goto End

:Failed
set PRIVATECONFIG=
echo ###########################################
echo ## ERROR: UNABLE TO LOCATE PRIVATECONFIG ##
echo ## LOADING / BUILDING PROJECTS WILL FAIL ##
echo ###########################################
call %~dp0\FlashConsole.cmd

:End
endlocal & set PRIVATECONFIG=%PRIVATECONFIG%
