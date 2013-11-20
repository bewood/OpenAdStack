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

::
:: Setup VS Environment
::
echo Setting up environment for Visual Studio 2012
call "%VS110COMNTOOLS%\..\..\VC\vcvarsall.bat" x86

::
:: Locate PrivateConfig
::
call %~dp0\LocatePrivateConfig.cmd
if exist "%PRIVATECONFIG%" (
  echo Using PrivateConfig at "%PRIVATECONFIG%"
)

::
:: Locate git (for inclusion in the path)
::
call %~dp0\LocateGitPath.cmd
if not exist "%GIT_PATH%" (
  echo WARNING: Unable to find git^^! Please set GIT_PATH to a valid path.
)

::
:: Add tool and build output directories to path
::
set PATH=%PATH%;%GIT_PATH%;%~dp0;%~dp0..\Public\bin\Debug;%~dp0..\Public\bin\Release

::
:: Chdir to the project root
::
cd %~dp0\..

::
:: Detect the current git branch
::
setlocal enabledelayedexpansion
for /f "tokens=1,2" %%a in ('git branch') do (
  if '%%a'=='*' (
    set BRANCH=%%b
    echo Current branch is '!BRANCH!'
  )
)
endlocal & set BRANCH=%BRANCH%

::
:: Trick git into using color in cmd
::
set TERM=cygwin
set LESS=FRSX
git config color.ui always

::
:: Check if the PrivateConfig found is the default setup template
:: If so, display a warning offer to open the setup guide.
:: 
setlocal enabledelayedexpansion
for /f "delims=" %%i in ('echo %PRIVATECONFIG% ^| find /i "build\Setup\PrivateConfig"') do (
  echo.
  echo WARNING: Current PrivateConfig is a default setup template which only supports
  echo          building, running unit test and the Azure dev fabric emulator.
  echo          See build\Setup\PrivateConfigSetupGuide.md for more information.
  if /i not "%OpenAdStack.SkipPrivateConfigSetupGuidePrompt%"=="True" (
    echo.
    echo Would you like to view the guide on GitHub now?
    choice /C YNL /T 15 /D L /N /M "[Y]es [N]o [L]ater?"
    if '!ERRORLEVEL!'=='1' start https://github.com/RareCrowds/OpenAdStack/blob/master/build/Setup/PrivateConfigSetupGuide.md
    if not '!ERRORLEVEL!'=='3' %~dp0\SetEnvVarHKCU.cmd "OpenAdStack.SkipPrivateConfigSetupGuidePrompt" "True"
  )
)
endlocal
