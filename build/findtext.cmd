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

if '%1'=='' goto Usage

setlocal

:ParseArgs
if not '%1'=='' (
  if /i '%1'=='-t' (
    set TEXT=%2
    shift /1
  )
  if /i '%1'=='-f' (
    set FILTER=%2
    shift /1
  )
  if '%1'=='-q' set QUIET=True
  if '%1'=='-?' goto Usage
  if /i '%1'=='--help' goto Usage
  shift /1
  goto ParseArgs
)

if '%TEXT%'=='' (
  echo Missing required parameter -t
  goto Usage
)

if '%FILTER%'=='' set FILTER=*
if '%QUIET%'=='' set QUIET=False

::
:: Search files matching the filter for the specified text
::
set FILES=0
for /f "delims=" %%i in ('dir /b /s /a:-d %FILTER% ^| find /i /v "\bin\" ^| find /i /v "\.git\" ^| find /i /v "\obj\"') do (
  for /f "tokens=1,2,3 delims=:" %%j in ('find /i /c %TEXT% "%%i"') do (
    if not "%%l"==" 0" (
      echo %%i
      set /a FILES=FILES + 1
    )
  )
)
if '%QUIET%'=='False' (
  echo.
  echo %FILES% found files containing %TEXT%.
  echo.
)
endlocal
goto End

:Usage
echo Recursively searches directories for files containing the specified text.
echo.
echo Usage: %~nx0 -t "text" [-f "file filter"] [-q]
echo   -t  Text to search for (required).
echo   -f  Filter string of files to seach (optional).
echo   -q  Quiet mode. Displays file names only.
echo.
echo Example:
echo %~nx0 -t "some text" -f "*.cs" -q
echo.

:End
