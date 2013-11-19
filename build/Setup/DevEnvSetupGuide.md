Open Ad Stack Development Environment Setup
===========================================
_NOTE: We have currently only tested with Visual Studio Express 2012 running on Window 7. Visual Studio Professional 2012/2013, Visual Studio Express 2013, and Windows 8 should also work, but have not been tested._

### Install Development Tools and SDKs
* [Visual Studio Express 2012 for Windows Desktop] [10]
* [Visual Studio Express 2012 for Web \(includes Azure SDK v2.1\)] [11]

### Install IIS
If IIS has not yet been installed on your system:
*	Run appwiz.cpl by typing it into the Start Menu
*	Click 'Turn Windows features on or off'
*	Expand 'Internet Information Services'
*	Check everything under 'World Wide Web Services' and 'Web Management Tools'
*	Click 'Okay'

### Enable IIS 6 Management Compatibility
Even if IIS has been installed, the IIS 6 Management Compatibility component may still need to be enabled:
* Run appwiz.cpl by typing it into the Start Menu
*	Click 'Turn Windows features on or off'
*	Expand 'Internet Information Services'
* Expand 'Web Management Tools'
* Expand 'IIS 6 Management Compatibility'
* Check 'IIS Metabase and IIS 6 configuration compatibility'
* Click 'Okay'

### Configure SQLExpress
* Run "SQL Server Configuration Manager" by typing it into the Start Menu
* Expand 'SQL Server network Configuration'
* Select 'Protocols for SQLEXPRESS'
* Double-click 'TCP/IP' in the right pane
* Under the 'Protocol' tab set 'Enabled' to Yes
* Under the 'IP Addresses' tab go to 'IPAll' (scroll to the bottom)
* For 'IPAll' set 'TCP Dynamic Ports' to 65000

### Private Config
Various components of the Open Ad Stack require configuration specific to your team's infrastructure (account credentials, connection strings, server endpoints, SSL certificates, etc).
This guide will get you up and running in the local Azure dev fabric emulator only and does not support deploying to Windows Azure or exporting campaigns to any ad networks.
Before you can deploy to Windows Azure or integrate with supported ad networks you must setup your team's privateconfig by following the [PrivateConfig Setup Guide] [2].

### Install local emulator ACS and SSL certificates
These certificates are included in the template privateconfig and allow developers to get up and running with the Open Ad Stack locally using the RareCrowds ACS namespace.
You will replace them with your team's own certificates later as part of the privateconfig setup.

#### Import the certificate for rcoasemu.accesscontrol.windows.net
* From Explorer run build/Setup/PrivateConfig/Azure/Certificates/Certificates(LocalComputer).msc
* Right-click 'Certificates (Local Computer)/Personal/Certificates'
* Select 'All Tasks' &rarr; 'Import...'
* Click Next then Browse...
* Select build/Setup/PrivateConfig/Azure/Certificates/ACS/rcoasemu.cer
* Click Next, Next, Finish, OK

#### Import the RSA cookies certificate (used by JWT/ACS)
* From Explorer run build/Setup/PrivateConfig/Azure/Certificates/Certificates(LocalComputer).msc
* Right-click 'Certificates (Local Computer)/Personal/Certificates'
* Select 'All Tasks' &rarr; 'Import...'
* Click Next then Browse...
* Change the filter from 'X.509 Certificate' to 'Personal Information Exchange'
* Select build/Setup/PrivateConfig/Azure/Certificates/ACS/rcoasemu.rsacookies.pfx and click Next
* Enter the private key password: `rsacookies4TestingOnly!`
* Click Next, Finish, OK

#### Import the oas.local SSL certificate
* From Explorer run build/Setup/PrivateConfig/Azure/Certificates/Certificates(LocalComputer).msc
* Right-click 'Certificates (Local Computer)/Personal/Certificates'
* Select 'All Tasks' &rarr; 'Import...'
* Click Next then Browse...
* Change the filter from 'X.509 Certificate' to 'Personal Information Exchange'
* Select build/Setup/PrivateConfig/Azure/Certificates/SSL/oas.local.pfx and click Next
* Enter the private key password: `oas.local4TestingOnly!`
* Click Next, Finish, OK

#### Grant IIS private key access to the SSL certificate
* From Explorer run build/Setup/PrivateConfig/Azure/Certificates/Certificates(LocalComputer).msc
* Select 'Certificates (Local Computer)/Personal/Certificates' in the navigation pane
* Right-click the certificate Issued To "oas.local"
* Select 'All Tasks' &rarr; 'Manage Private Keys...'
* Click 'Add...' then enter `NETWORK SERVICE` in the box and click 'Check Names'
* If typed correctly the name should appear underlined now
* Click OK and then OK again

### Development Command-line and Building
After dependencies are installed, and environment setup steps have been followed:
* Launch [env.cmd] [1] as administrator (right-click &rarr; Run as administrator)
* Run `BuildDebug` to build all solutions
* Verify everything builds without errors

For quick access we recommend creating an administrator shortcut to env.cmd:
* Open your local copy of OpenAdStack in Explorer
* Hold Alt and drag env.cmd to your desktop
* Right-click the new shortcut and select 'Properties &rarr; Shortcut &rarr; Advanced...'
* Check 'Run as administrator'
* Press 'OK' to close the Advanced Properties dialog
* Press 'OK' again to close the shortcut properties

Run `buildall -?` for more information on the various ways the Open Ad Stack can be built.


Build and run in the Windows Azure Dev Fabric emulator
------------------------------------------------------
### Add oas.local to etc/hosts
Entries must be added to etc/hosts for the Open Ad Stack to function correctly with SSL, ACS and Microsoft Connect in the local dev fabric emulator.

A [convinience script] [4] has been provided to add the entries if they are not already present:
* Launch [env.cmd] [1] as administrator
* Run `build\Setup\AddOasLocalToEtcHosts.cmd`

Alternatively you may add the entries manually:
* Launch Notepad as administrator
  * Open the Start Menu, type "Notepad"
  * From the Programs list right-click Notepad
  * Select "Run as administrator"
* In Notepad open C:\Windows\System32\drivers\etc\hosts
* Add the following to the end of the file:

        127.0.0.1 oas.local
        1:: oas.local

* Save the file and close Notepad

**Note:** Your anti-virus may display an alert that the file has been altered. Select dismiss/ignore/allow exception or similar to leave your changes intact.

### Register a Microsoft Account
The Open Ad Stack is currently setup to use Microsoft Accounts (via Azure ACS) for authentication. Support for other identity providers supported by ACS may be added in the future.

You will need at least one Microsoft Account for the default administrator identity.
To register a Microsoft Account go to https://signup.live.com/, fill out the form and click the link in the subsequent confirmation email.
It is recommended that you register this account to a developer/administrator distribution list (ex: oasdevftes@myteam.org) rather than an individual developer's email account.

_Adding this account as your team's default administrator is covered in the [PrivateConfig Setup Guide] [2]_

### Running the Open Ad Stack in the dev fabric emulator
* Launch env.cmd as administrator (right-click &rarr; Run as administrator)
* Run `BuildDebug NoFxCop NoXmlDoc & InitStorage & Azure\Azure.sln`
  * If presented with the Visual Studio version selector pick "Microsoft Visual Studio Express 2012 for Web"
  * **Note:** You do not have to rebuild the world every time, this just for initial setup
* In Solution Explorer, set the "Azure" cloud project as the StartUp project (right-click &rarr; Set as StartUp Project)
* Press F5 to run the Open Ad Stack web and work roles in the Azure dev fabric emulator
* Open a browser to https://oas.local/ and sign-in using your default Microsoft Account
  * Ignore any errors in browsers automatically launched by Visual Studio

At this point, if you have not yet completed the [PrivateConfig Setup Guide] [2] sign-in will fail.
However, here's a quick hack to get up and running in your local environment.
* Follow the steps above to launch the Open Ad Stack in your dev fabric emulator
* Open the Compute Emulator UI by right-clicking the Azure taskbar icon (blue Windows logo)
* In the navigation pane select Azure\WorkerRole\0
* The right side should display a screen full of log messages.
* Scroll to the bottom and wait until it calms down.
* Open a clean browser (cleared cookies/cache) to https://oas.local/
* Sign in using your Microsoft Account
  * If prompted, grant permission for oas.local to access your Microsoft Account
* Sign in should fail (currently you will just be redirected back to the sign-in page)
* Switch back to the Compute Emulator UI (which should be active again)
* Locate the trace message containing the "User not found" error

        <ActivityResult xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/Activities">
          <Error>
            <ErrorId>8</ErrorId>
            <Message>User not found: 'qwertfZ7OJR4yz0DFnSFD+p57Xc3FSDFOIRe2MUSjvo='</Message>
          </Error>
          <RequestId>GetCompaniesForUser ()</RequestId>
          <Succeeded>false</Succeeded>
          <Task>GetCompaniesForUser</Task>
          <Values xmlns:d2p1="http://schemas.microsoft.com/2003/10/Serialization/Arrays" />
        </ActivityResult>

* Copy the base64 value from the error message ('qwertfZ7...')
* Close the Compute Emulator UI and Visual Studio
* Right-click the Azure taskbar icon and select Exit and press OK if prompted
* Switch back to your Open Ad Stack command-line
* Run `initstorage AdminUserId "qwertfZ7OJR4yz0DFnSFD+p57Xc3FSDFOIRe2MUSjvo="` (replacing with your base64 user id value)
* Run `Azure\Azure.sln` and press F5

You should now be able to log in with your Microsoft Account. When the Company Page has loaded try creating a New Account (only the Name value is required).
Once your test company displays in the Company Page list you have successfully authenticated and created an object in the system as the default administrator.

### Continuous Integration Builds
The Open Ad Stack build scripts include support for [TeamCity] [12]
See the [OpenAdStack TeamCity Guide] [3] for details on how to setup CI builds.

Code Contribution Guidelines
----------------------------
### New projects
All projects must be instrumented to use our custom build system. When adding a new assembly you must manually edit the .csproj and replace the Microsoft.CSharp.targets import
```XML
  <Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" />
```
with a reference to our custom [common.proj] (/build/common.proj):
```XML
  <Import Project="$(SolutionDir)..\build\common.proj" />
```
Additionally, if custom `"BeforeBuild"` or `"AfterBuild"` targets are needed they must be renamed to `"ProjectBeforeBuild"` and `"ProjectAfterBuild"`.

Also, when creating new projects, VisualStudio automatically uses the registered organization of your computer for the company value in AssemblyInfo.cs.
Please makes sure if you create new AssemblyInfo.cs files you copy the values from an existing [AssemblyInfo.cs] (/Common/Utilities/Properties/AssemblyInfo.cs) to reflect the Open Source project copyright.

### Coding standards
All code must pass StyleCop and FxCop analysis. Limted exceptions are allowed on a case-by-case basis using the `System.Diagnostics.CodeAnalysis.SuppressMessage` attribute.
The StyleCop/FxCop configurations may not be modified, with the exception of additions to the custom dictionaries (such as adding a new ad network name).

[1]: /env.cmd "Development environment launcher script"
[2]: /build/Setup/PrivateConfigSetupGuide.md "PrivateConfig Setup Guide"
[3]: /build/Setup/TeamCitySetupGuide.md "TeamCity Setup Guide"
[4]: /build/Setup/AddOasLocalToEtcHosts.cmd "Adds oas.local mappings to etc/hosts"
[10]: http://www.microsoft.com/en-us/download/details.aspx?id=34673 "Download the Visual Studio Express 2012 for Windows Desktop installer from Microsoft"
[11]: http://www.microsoft.com/en-us/download/details.aspx?id=30669 "Download the Visual Studio Express 2012 for Web installer from Microsoft"
[12]: http://www.jetbrains.com/teamcity "TeamCity website"
