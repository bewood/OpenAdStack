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

setlocal ENABLEDELAYEDEXPANSION

::::::::::::::::::::::::::::::
:: Default settings
::::::::::::::::::::::::::::::
set force=false
set source=
set dtype=
set dconn=

::::::::::::::::::::::::::::::
:: Parse arguments
::::::::::::::::::::::::::::::
:ParseArgs
if /i not '%1'=='' (
  :: Upload even if an already exists
  if /i '%1'=='-f' set force=true
  :: Persistent data upload source path
  if /i '%1'=='-s' (
    set source=%2
    shift /1
  )
  :: Dictionary connection string (override dview default)
  if /i '%1'=='-cs' (
    set dconn=%2
    shift /1
  )
  :: Dictionary type (required for -cs)
  if /i '%1'=='-dt' (
    set dtype=%2
    shift /1
  )
  
  shift /1
  goto ParseArgs
)

::::::::::::::::::::::::::::::
:: Check arguments
::::::::::::::::::::::::::::::
if not exist "%source%" (
  echo The source "%source%" does not exist.
  goto Help
)
if not '%dconn%'=='' if '%dtype%'=='' (
  echo Missing dictionary type
  goto Help
)

:::::::::::::::::::::::::::::::::::::::
:: Upload files from source directory
:::::::::::::::::::::::::::::::::::::::
echo Uploading persistent data from "%source%":
pushd %source%
for /f "delims=" %%i in ('dir/b/s/a:-d') do if not '%~dpnx0'=='%%i' (
  set file=%%i
  set entry=!file:%CD%\=!
  for /f "tokens=1,2 delims=\" %%j in ("!entry:%CD%\=!") do (
    set store=%%j
    set key=%%k
    
    set upload=false
    echo !file!
    if %force%==true (
      set upload=true
    ) else (
      REM Only upload if the entry does not already exist
      echo     Checking if entry exists for !store!/!key!...
      dview -c view -s "!store!" -k "!key!">NUL
      if !ERRORLEVEL! GTR 0 (
        set upload=true
      ) else (
        echo     Skipping existing entry !store!/!key!
      )
    )
    
    if !upload!==true (
      echo     Uploading !store!/!key!
      dview -c set -s "!store!" -k "!key!" -i "!file!"
    )
  )
)
popd
goto END

:Help
echo %~nx0 -s "PATH\TO\SOURCE" [-f] [-cs "CONNECTION_STRING" -t Sql^|Cloud]
echo   -s  Path to the source files to upload as entries (in subfolders corresponding to stores)
echo   -f  Force upload even if an entry already exists
echo   -cs Connection string (overrides dview default)
echo   -t  Dictionary type (required for -cs, see dview for details)

:END
endlocal
