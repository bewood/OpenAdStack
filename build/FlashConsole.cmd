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
:: Simple script to flash the console in event of build script errors
::
set FLASH=4F
set CURRENT=%FLASH%
for /l %%i in (0,1,10) do (
  if '!CURRENT!'=='' (
    set CURRENT=%FLASH%
  ) else (
    set CURRENT=
  )
  color !CURRENT!
  for /l %%j in (0,1,800) do echo.>NUL
)

endlocal