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

:: Check if GIT_PATH is already set and valid
if exist "%GIT_PATH%" if exist "%GIT_PATH%\git.exe" (
  goto :EOF
) else (
  echo WARNING: GIT_PATH not set or invalid
  echo     Searching for git.exe...
)

:: If not, try a default path
set GIT_PATH=C:\Program Files (x86)\Git\bin
:: If the default isn't valid search the system drive
if not exist "%GIT_PATH%" (
  pushd %SystemDrive%\
  for /f "delims=" %%i in ('dir /b /s git.exe ^| find /i "\bin\"') do (
    if exist "%%i" (
      set GIT_PATH="%%~dpi"
      goto FoundGit
    )
  )
  echo WARNING: Unable to locate git.exe
)
:FoundGit
popd
echo     Located git.exe at "%GIT_PATH%"
echo.
choice /T 6 /D n /M "Save GIT_PATH for future use"
if %ERRORLEVEL%==1 (
  call %~dp0\SetEnvVarHKCU.cmd "GIT_PATH" "%GIT_PATH:\=\\%"
  echo     Saved GIT_PATH to user environment variables.
)
echo.

