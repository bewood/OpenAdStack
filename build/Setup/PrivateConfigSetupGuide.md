Open Ad Stack PrivateConfig Setup Guide
=======================================
Various components of the Open Ad Stack require configuration specific to your team's infrastructure (account credentials, connection strings, server endpoints, SSL certificates, etc).
Included below are steps for creating a PrivateConfig directory from the included template, setting up the prerequisite accounts and adding them to your team's PrivateConfig.
This guide is divided into 4 sections:
### [Prerequisite Accounts] (#prerequisite-accounts-1)
Accounts that you will need to run a functional Azure deployment of the Open Ad Stack
### [Azure Dev Environment] (#azure-dev-environment-1)
How to setup an Azure Dev environment for your team
### [Team PrivateConfig Setup] (#team-privateconfig-setup-1)
How to create a PrivateConfig for use by your team
### [Local PrivateConfig Setup] (#local-privateconfig-setup-1)
How to configure a local dev environment to use your team's PrivateConfig

If your team has already setup its PrivateConfig skip to the [local setup guide] (#privateconfig-local-setup).

----------------------

Prerequisite Accounts
---------------------
### Basic Azure deployment
#### Azure Developer Account (http://www.windowsazure.com)
* Included in some MSDN subscriptions
* Free 1-month trial available

#### Microsoft Accounts (http://signup.live.com)
Microsoft Accounts are used for authentication within the Open Ad Stack as well as for managing other Microsoft technologies used by the Open Ad Stack.
* Personal developer account
  * For the Azure subscription
* Default administrator account
  * Used for both local and cloud deployments
  * Shared with your team
  * Recommend using a distribution list email address instead of an individual developer's
  * May also be used for Microsoft Live Connect

----------------------

#### SMTP (SendMail) Account
The Open Ad Stack uses SMTP for sending real-time operations alerts as well as registration requests/confirmations.
For development SendGrid's free package works well: https://sendgrid.com/user/signup/package/76

----------------------

### Ad Networks
The Open Ad Stack currently supports exporting campaigns to AppNexus (for advertisers) and Google DFP (for publishers).
A test environment registration for at least one ad network is required in order to build campaigns
(the campaign builder needs an ad network to provide targeting persona measures).

#### AppNexus: http://www.appnexus.com/contact
* For "Type of Business:" select "Agency"
* For "Interested in:" select "Buying"
* For "No. of Imps" just put in something like 100.
When an account specialist contacts you, tell them you're currently just interrested in a "sandbox" account for prototyping programatic buying software.

#### Google DFP: https://developers.google.com/doubleclick-publishers/
* Follow steps 1-3 of their [Getting Started] (https://developers.google.com/doubleclick-publishers/docs/start) guide to setup a test network
* Create a google account to share with your team for testing (for step 2)
* The DFP client libraries are already integrated in the Open Ad Stack (stop at step 4)

----------------------

### Payment Processing
Before the Open Ad Stack exports a campaign it verifies that it has approved billing information for the customer.
The Open Ad Stack currently supports payment processing using stripe.com.

A new stripe.com account for testing can be quickly created at https://manage.stripe.com/register

In a local test environment billing approval may be bypassed with a using the ProcessPayment.exe utility:
* Build and run the Open Ad Stack in the dev fabric emulator
* Create and configure a campaign so that it is ready to Go Live
* Copy the campaign's ID from the campaign manager page URL

        https://oas.local/campaign.html?agency=9b3e64...6fe9&advertiser=54c883...b507&campaign=a03c9d3ad8b546738d3a77409790e73a

* From the env command-line run `ProcessPayment` with the Test Approval flag
  
        ProcessPayment -ta -campaign a03c9d3ad8b546738d3a77409790e73a

* You may now hit Go Live to approve the campaign for export

----------------------

Azure Dev Environment
---------------------
Now that external accounts have been registered it is time to setup your team's deployed Azure development environment.
The following steps will all be performed within the [Windows Azure management portal] (https://manage.windowsazure.com).
The portal can often become slow to respond, however, it is rare for operations to actually fail. For the sake of your blood pressure/mental health, I highly recommend having some sort of other work or a puzzle game at hand.
The good news is that once these steps are completed you will be able to build and deploy to Azure with a single command from the development command-line and will only need visit the portal to shut down unused deployments.

A deployment of Open Ad Stack in Azure consists of the following components:
* [Cloud Service] (#cloud-service)
* [Storage Account] (#storage-account)
* [SQL Database] (#sql-database)
* [ACS Namespace] (#acs-namespace)

### Cloud Service
This is where the Open Ad Stack code is deployed.
Depending on your Azure subscription, you may be billed for deployments within the Cloud Service even if they are stopped.
*Be sure to delete all deployments under this service when not in use.*

### Storage Account

### SQL Database

### ACS Namespace

Team PrivateConfig Setup
------------------------
### Gather prerequisite account information



PrivateConfig Local Setup
-------------------------
*TODO: Steps for setting up local privateconfig (bewood 20131115T0140Z)*
