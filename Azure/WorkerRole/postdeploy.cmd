:: Skip if running in the emulator
if /i "%EMULATED%"=="true" goto :EOF

:: Deploy the Visual Studio 2010 CRT (required for OpenSSL)
set isInstalled = 1
reg query HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x64 /v Installed ||  set setisInstalled = 0

if isInstalled == 0 (
    echo Installing VS2010 runtime
    vcredist_x64.exe /q /norestart
)
