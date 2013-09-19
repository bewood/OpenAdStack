::@echo off

::
:: Setup VS Environment
::
call "%VS100COMNTOOLS%\..\..\VC\vcvarsall.bat" x86

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

echo.
