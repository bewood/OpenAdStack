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

set SOLUTIONDIR=%1
set PROJECTDIR=%2
set OUTDIR=%3

set FILES_TO_COPY=%PROJECTDIR%\FilesToCopy.txt
set XCOPYFLAGS=/EVIFHY

pushd %SOLUTIONDIR%

if exist "%OUTDIR%" rmdir /q /s "%OUTDIR%"

for /f "eol=# tokens=1,2 delims=^|" %%i in (%FILES_TO_COPY%) do (
  set DEST=%OUTDIR%
  if not "%%j"=="" set DEST=%OUTDIR%\%%j
  if not exist "!DEST!" mkdir "!DEST!"
  xcopy %XCOPYFLAGS% "%%i" "!DEST!"
)

popd
endlocal
