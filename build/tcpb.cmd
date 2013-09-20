@echo off

:: Set local environment
setlocal enabledelayedexpansion

:: Parse arguments
if '%1'=='' goto Help

:ParseArgs
if not '%1'=='' (
  if /i '%1'=='info' set INFO=true
  if /i '%1'=='clear' set CLEAR=true
  if /i '%1'=='build' (
    set BUILD=true
    set CONFIG=%2
    shift /1
  )
  if /i '%1'=='-b' (
    set DEST=%2
    shift /1
  )
  shift /1
  goto ParseArgs
)

:: Check that at least one valid function was specified
if '%INFO%%CLEAR%%BUILD%'=='' goto Help

set TC.CONFIG=%USERPROFILE%\.openadstack\teamcity.cfg

:: Display saved configuration
if /i '%INFO%'=='true' (
  if exist "%TC.CONFIG%" (
    echo Saved configuration found at "%TC.CONFIG%":
    type "%TC.CONFIG%"
    echo.
  ) else (
    echo No saved configuration found at "%TC.CONFIG%"
  )
)

:: Clear saved configuration
if /i '%CLEAR%'=='true' (
  if exist "%TC.CONFIG%" (
    echo Deleting saved configuration at "%TC.CONFIG%"
    del /f "%TC.CONFIG%"
    set TC.USER=
  ) else (
    echo No saved configuration found at "%TC.CONFIG%"
  )    
)

:: Submit a personal build to TeamCity via ::ote-run branch
if /i '%BUILD%'=='true' (

  :: Load any saved configuration
  if exist "%TC.CONFIG%" (
    for /f "tokens=1 delims=" %%i in (%TC.CONFIG%) do set TC.%%i
  ) else (
    if not exist "%USERPROFILE%\.openadstack" mkdir "%USERPROFILE%\.openadstack"
  )

  :: Prompt for TeamCity user if not loaded from config
  if '!TC.USER!'=='' (
    set /p TC.USER=TeamCity User:
    if '!TC.USER!'=='' (
      echo TeamCity user name is required.
      echo.
      goto Help
    )
    echo USER=!TC.USER!>>"%TC.CONFIG%"
  )

  :: Submit the push
  if '%CONFIG%'=='' (
    echo Please specify a TeamCity build configuration.
  )
  if '%DEST%'=='' (
    for /f "tokens=1,*" %%a in ('git branch') do (
      if '%%a'=='*' set DEST=%%b
    )
  )
  git push origin +HEAD:remote-run/%CONFIG%/!TC.USER!/!DEST!
)

goto End

:Help
echo Script to submit TeamCity personal builds using Branch ::ote Run Triggers
echo %~n0 {info^|build CONFIGURATION [-s SOURCE_BRANCH] [-b DESTINATION_BRANCH]}
echo info  Display saved settings (if any)
echo clear Clear any saved settings
echo build Build the specified configuration
echo    -b ::ote destination branch (default is same as local active branch)
echo.
echo This script is simply a wrapper for issuing a 'git push' to a ::ote branch
echo monitored by a TeamCity Branch ::ote Run Trigger. For example, assuming the
echo active local branch is 'my_feature', the following invocation:
echo   %~n0 build bvt
echo would result in a git push similar to this:
echo   git push origin +HEAD:::ote-run/bvt/joedeveloper/my_feature
echo.
echo The teamcity user name will be prompted for if not already set and then saved
echo to a per-user configuration file. Use '%~n0 info' to display saved settings.
echo This script requires corresponding Branch ::ote Run Triggers to be configured
echo in TeamCity monitoring refs/heads/::ote-run/CONFIGURATION/TEAMCITY_USERNAME/*
echo.

:End
endlocal
