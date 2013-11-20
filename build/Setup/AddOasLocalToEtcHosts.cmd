@echo off
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Adds entries for oas.local to etc/hosts if not already present
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
setlocal
set ETCHOSTS=%SystemRoot%\System32\drivers\etc\hosts

:: Check if already present
for /f %%i in ('type %ETCHOSTS% ^| find "oas.local"') do (
  echo Entries for oas.local already present in %ETCHOSTS%
  goto End
)

:: Add entries
echo.>>%ETCHOSTS%
echo ########################################################>> %ETCHOSTS%
echo # Open Ad Stack oas.local mappings for Azure Dev Fabric >> %ETCHOSTS%
echo ########################################################>> %ETCHOSTS%
echo 127.0.0.1 oas.local>>%ETCHOSTS%
echo ::1 oas.local>>%ETCHOSTS%
echo.>>%ETCHOSTS%

echo Added mappings for oas.local to %ETCHOSTS%

:End
endlocal
