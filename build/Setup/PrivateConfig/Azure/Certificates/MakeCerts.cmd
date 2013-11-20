@echo off
::-------------------------------------------------------------------------------------------------
set RootCommonName=Rare Crowds Inc
set CommonName=Rare Crowds RSA Cookies
set Output=rsacookies.dev

::-------------------------------------------------------------------------------------------------
:: Create the root certificate
REM makecert.exe -n "CN=%RootCommonName%,O=Rare Crowds Inc,OU=Open Ad Stack,L=Bellevue,S=WA,C=US" -pe -ss my -sr LocalMachine -sky exchange -m 96 -a sha1 -len 2048 -r RareCrowdsRoot.cer
:: Import Root Certificate Authority Certificate into Trusted Root Store:
REM certutil.exe -f -addstore Root RareCrowdsRoot.cer
:: Create Backup (Export) PFX file of Root Certificate Authority Certificate:
REM certutil.exe -privatekey -p Traffiq!RC -exportpfx "%RootCommonName%" RareCrowdsRoot.pfx

::-------------------------------------------------------------------------------------------------
:: Create a Certificate issued from the previously created Certificate Authority:
makecert.exe -n "CN=%CommonName%,O=Rare Crowds Inc,OU=Open Ad Stack,L=Bellevue,S=WA,C=US" -pe -ss my -sr LocalMachine -sky exchange -m 96 -in "%RootCommonName%" -is my -ir LocalMachine -a sha1 -eku 1.3.6.1.5.5.7.3.1,1.3.6.1.5.5.7.3.2 %Output%.cer
:: Create Backup (Export) PFX file of Server Certificate
certutil.exe -privatekey -p Traffiq!RC -exportpfx "%CommonName%" %Output%.pfx

