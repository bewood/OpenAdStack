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
setlocal

::
:: Set user environment variables in registry
:: Note: Values containing \ and " must be pre-escaped
::

set VarName=%~1
set VarValue=%~2

set EnvVarReg=%TEMP%\%~n0[%RANDOM%].reg
echo Windows Registry Editor Version 5.00>%EnvVarReg%
echo.>>%EnvVarReg%
echo [HKEY_CURRENT_USER\Environment]>>%EnvVarReg%
if not "%VarValue:"=%"=="" (
  echo "%VarName%"="%VarValue%">>%EnvVarReg%
) else (
  echo "%VarName%"=->>%EnvVarReg%
)
regedit /s %EnvVarReg%
del /f %EnvVarReg%
endlocal
