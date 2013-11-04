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

setlocal disabledelayedexpansion
pushd %~dp0

set TARGETSERVER=-S .\SQLEXPRESS
set USERSWITCH=
set PSWDSWITCH=
set SQLCOMMANDPARAMS=
set TABLESCRIPTFILES=
set STATICDATASCRIPTFILES=
set SPSCRIPTFILES=
set DBSCRIPTFILES=
set USERSCRIPTFILES=

::::::::::::::::::::::::::::::
:: Parse Args
::::::::::::::::::::::::::::::
if '%1'=='' goto Help

:ParseArgs
if not '%1'=='' (
	if /i '%1'=='-s' (
		set TARGETSERVER=-S %2
		shift /1
	)

	if /i '%1'=='-u' (
		set USERSWITCH=-U %2
		shift /1
	)
	
	if /i '%1'=='-p' (
		set PSWDSWITCH=-P %2
		shift /1
	)

	if /i '%1'=='-create' (
		set DBSCRIPTFILES=..\Scripts\Database\create\create_*.sql
		set USERSCRIPTFILES=..\Scripts\User\create\create_*.sql
		set TABLESCRIPTFILES=..\Scripts\Tables\update\update_*.sql
		set STATICDATASCRIPTFILES=..\Scripts\StaticData\update\update_*.sql
		set SPSCRIPTFILES=..\Scripts\StoredProcedures\update\update_*.sql
	)
	
	if /i '%1'=='-update' (
		set DBSCRIPTFILES=
		set USERSCRIPTFILES=
		set TABLESCRIPTFILES=..\Scripts\Tables\update\update_*.sql
		set STATICDATASCRIPTFILES=..\Scripts\StaticData\update\update_*.sql
		set SPSCRIPTFILES=..\Scripts\StoredProcedures\update\update_*.sql
	)
	
	if /i '%1'=='-delete' (
		set DBSCRIPTFILES=
		set USERSCRIPTFILES=
		set TABLESCRIPTFILES=..\Scripts\Tables\delete\delete_*.sql
		set STATICDATASCRIPTFILES=
		set SPSCRIPTFILES=
	)
	 
	if '%1'=='/?' goto Help 
	if '%1'=='-?' goto Help
	if /i '%1'=='/help' goto Help
	if /i '%1'=='--help' goto Help
	
	shift /1
	goto ParseArgs
)

:Run

set SQLCOMMANDPARAMS=%TARGETSERVER% %USERSWITCH% %PSWDSWITCH%

::::::::::::::::::::::::::::::
:: Run database scripts
::::::::::::::::::::::::::::::
for %%S in (%DBSCRIPTFILES%) do sqlcmd -d master %SQLCOMMANDPARAMS%  -i %%S 

::::::::::::::::::::::::::::::
:: Run user scripts
::::::::::::::::::::::::::::::
for %%S in (%USERSCRIPTFILES%) do sqlcmd -d IndexDatastore %SQLCOMMANDPARAMS%  -i %%S 

::::::::::::::::::::::::::::::
:: Run table scripts
::::::::::::::::::::::::::::::
for %%S in (%TABLESCRIPTFILES%) do sqlcmd -d IndexDatastore %SQLCOMMANDPARAMS% -i %%S 

::::::::::::::::::::::::::::::
:: Run stored procedure scripts
::::::::::::::::::::::::::::::
for %%S in (%SPSCRIPTFILES%) do sqlcmd -d IndexDatastore %SQLCOMMANDPARAMS% -i %%S 

::::::::::::::::::::::::::::::
:: Run static data scripts
::::::::::::::::::::::::::::::
for %%S in (%STATICDATASCRIPTFILES%) do sqlcmd -d IndexDatastore %SQLCOMMANDPARAMS% -i %%S 

goto End

::::::::::::::::::::::::::::::
:: Help Message
::::::::::::::::::::::::::::::
:Help
echo Runs the Entity Index database install scripts.
echo.
echo Usage: -s serverinfo [-u username] [-p password] [-create^|-update^|-delete]
echo 
echo -s				serverinfo parameter option (required) 
echo serverinfo		sqlcmd -S compatible server spec. Can be '.'
echo -u				optional Sql Auth username (if not specified will use windows auth)
echo username		username (for cloud deployment must be of the form username@server
echo -p				optional Sql Auth password if -u is specified and not -p, it will prompt
echo password		password Sql Auth

:End
popd
endlocal
