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
:: Find Git and add to path
::
if not "%GIT_PATH%"=="" goto SetPath

:: TODO: Search for Git
set GIT_PATH=C:\Program Files (x86)\Git\bin

::
:: Add build directory to path
::
:SetPath
set PATH=%PATH%;%GIT_PATH%;%~dp0;%~dp0..\Public\bin\Release;%~dp0..\Public\bin\Debug;

::
:: Chdir to the project root
::
cd %~dp0\..

::
:: Set the branch
::
setlocal enabledelayedexpansion
for /f "tokens=1,2" %%a in ('git branch') do (
  if '%%a'=='*' (
    set BRANCH=%%b
    echo Current branch is '!BRANCH!'
  )
)
endlocal

::
:: Trick git into using color in cmd
::
set TERM=cygwin
set LESS=FRSX
git config color.ui always
