@echo off
setlocal
echo Killing MSBuild processes
for /f "tokens=1,2 delims= " %%i in ('tasklist ^| find /i "msbuild"') do (
  echo Killing PID %%j
  taskkill /f /pid %%j
)
endlocal
