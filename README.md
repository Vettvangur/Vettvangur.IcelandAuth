# Vettvangur.IcelandAuth <sup>v2.1.0</sup>

Vettvangur.IcelandAuth is an open-source .Net [NuGet library](https://www.nuget.org/packages/Vettvangur.IcelandAuth/) intended to simplify integrating with the island.is authentication service.

island.is's free authentication service allows for authenticating icelandic nationals using digital certificates / Íslykill.

A prior contract with island.is is needed for production use, see https://island.is/#permit-884. General information on the island.is authentication service can be found [here](https://island.is/um-island-is/innskraningarthjonustan/).

This project was developed according to fixes and suggestions from [Syndis](https://www.syndis.is/) and influenced in part by the work done [here](https://github.com/digitaliceland/innskraningar-daemi/).

# ToC
- [Getting Started](#getting-started)
  * [Running the samples locally](#running-the-samples-locally)
  * [Requirements](#requirements)
  * [Installation & Usage](#installation---usage)
    * [AspNet Core](#aspnet-core)
    * [AspNet Umbraco 7/8](#aspnet-umbraco-7-8)
  * [Signature verification (skip when targetting Net5)](#signature-verification--skip-when-targetting-net5-)
- [Configuration](#configuration)
  * [Runtime Configuration](#runtime-configuration)
  + [Asp.Net Core appSettings.json structure](#aspnet-core-appsettingsjson-structure)
  + [Umbraco specific configuration](#umbraco-specific-configuration)
  + [Configuration - From island.is contract](#configuration---from-islandis-contract)
    * [Ensure SAML response url matches SAML response url destination. Corresponds with "Innskráningarsíða" from the control panel - Required](#ensure-saml-response-url-matches-saml-response-url-destination-corresponds-with--innskr-ningars--a--from-the-control-panel---required)
    * [The SSN used in the contract with island.is. - Required](#the-ssn-used-in-the-contract-with-islandis---required)
    * [The island.is contract id. - Required if using the url helper](#the-islandis-contract-id---required-if-using-the-url-helper)
  + [Configuration - Additional options](#configuration---additional-options)
    * [Constrain authentication method used on island.is.](#constrain-authentication-method-used-on-islandis)
    * [Route using AuthID attribute](#route-using-authid-attribute)
    * [Verify IP Address](#verify-ip-address)
    * [Log the Saml response received from island.is](#log-the-saml-response-received-from-islandis)
  + [Configuration - Umbraco packages](#configuration---umbraco-packages)
    * [Configure an absolute or relative url to redirect successful or erronous logins to.](#configure-an-absolute-or-relative-url-to-redirect-successful-or-erronous-logins-to)
- [.Net 5](#net-5)
- [Contribution](#contribution)
- [License](#license)
- [Other](#other)

<small><i><a href='http://ecotrust-canada.github.io/markdown-toc/'>Table of contents generated with markdown-toc</a></i></small>

# Getting Started

## Running the samples locally
Feel free to try out the samples locally configure with our test island.is contract. 

Samples listen on port 80. You will also need to configure a dns record mapping:

icelandauth.localhost -> 127.0.0.1

Umbraco samples require the IIS Url Rewrite module 

## Requirements
.Net Framework 4.6.1 or .Net Core 2.1.

Some Microsoft.Extensions packages will be installed if missing. They have a light footprint and are used for abstractions made available in .Net Standard.

## Installation & Usage
Grab the NuGet from [here](https://www.nuget.org/packages/Vettvangur.IcelandAuth/)

It's best to follow the appropriate sample depending on the framework you target, but these are the basic steps:

##### AspNet Core
Install core library NuGet
Add services.AddIcelandAuth() to Startup
Configure library using appSettings.json
Use in view or controller
	@inject IcelandAuthService AuthService

##### AspNet Umbraco 7/8
Install appropriate Umbraco integration NuGet
Configure library using Web.config
Hook into the ControllerBehavior.Success and Error events to handle authentication events

## Signature verification (skip when targetting Net5)

To be able to verify the signature returned from island.is you will need to install the audkenni certificate chain, http://www.audkenni.is/adstod/skilrikjakedjur.cfm.

The certificate Auðkennisrót needs to be installed into trusted roots in the server hosts certificate store.

The intermediate certificates should be added to Intermediate Certification Authorities.

Installing root certificates is outside the scope of this documentation but a detailed step-by-step can for example be found [here](https://docs.microsoft.com/en-us/skype-sdk/sdn/articles/installing-the-trusted-root-certificate).

[The .Net 5](#net5) version utilises the CustomRootTrust option to build a chain and comes bundled with the audkenni certificates. 

# Configuration

The sample projects in this repository show how to integrate IcelandAuth with AspNetCore/Umbraco. The Umbraco samples use the core IcelandAuth library and additional umbraco helpers from Vettvangur.IcelandAuth.Umbraco7[/8].

IcelandAuth is configured using appSettings key values, these are commonly stored in Web.config for Asp.Net and appSettings.json for Asp.Net Core projects.

## Runtime Configuration
It is also possible to override configured values using the public properties of the IcelandAuthService.

### Asp.Net Core appSettings.json structure

```json
{
    "IcelandAuth": {
        "ID": "",
        "Destination": "",
    }
}
```

### Umbraco specific configuration

See a live demo of the Umbraco 8 sample [here](https://icelandauth.vettvangur.is).

The Umbraco controllers listen for tokens on /umbraco/surface/icelandauth/login. 

Code under App_Start shows how to auto-provision users from island.is authentication data.

### Configuration - From island.is contract

The following options come from your island.is contract, you can view those values in the [island.is control panel](https://innskraning.island.is/thjonustuveitendur/Login.aspx?ReturnUrl=%2fthjonustuveitendur%2f). 

##### Ensure SAML response url matches SAML response url destination. Corresponds with "Innskráningarsíða" from the control panel - Required
```js
IcelandAuth:Destination - "http://icelandauth.localhost/icelandauth"
```
##### The SSN used in the contract with island.is. - Required
```js
IcelandAuth:DestinationSSN - "5208130550"
```

##### The island.is contract id. - Required if using the url helper
```js
IcelandAuth:ID - "test.icelandauth.vettvangur.is"
```

### Configuration - Additional options

##### Constrain authentication method used on island.is.

Possible values include:

* Rafræn skilríki – Digital certificate authentication.
* Rafræn símaskilríki - Digital certificate authentication using a phone.
* Rafræn starfsmannaskilríki – Employee digital certificate authentication.
* Íslykill – Authentication using Íslykill.
* Styrktur Íslykill – 2FA using Íslykill, 2FA delivered via phone or email.
* Styrkt rafræn skilríki – Digital certificate authentication with 2FA via phone/email.
* Styrkt rafræn starfsmannaskilríki – Employee digital certificate authentication with 2FA via phone/email.

Seperate multiple values with comma
```js
IcelandAuth:Authentication - "Rafræn skilríki, Rafræn símaskilríki"
```

##### Route using AuthID attribute
If configured and added to the island.is authentication url (the url helper will do this automatically) it will be echoed back in saml response.

It is possible to use this as a form of routing.

F.x. a site with is/en/dk section using the same domain and a single island.is contract id. 

If you pick a Guid for each section you can route the user based on the AuthID attribute in Saml response
```js
IcelandAuth:AuthID - "1d5e8fc3-1c02-4d9c-998c-7dd7f0ecc769"
```
Only Guid values are supported.

##### Verify IP Address
Check if the users IP matches the one seen at authentication.
This usually fails during development as island.is will see the public ip of the development machine. Meanwhile the development server, if hosted on your internal network, will see your intranet address.

Recommendation is to disable in development and enable live
```js
IcelandAuth:VerifyIPAddress - true // default
```
##### Log the Saml response received from island.is
Take care to only enable this option in development!
```js
IcelandAuth:LogSamlResponse - false // default
```

### Configuration - Umbraco packages

The following settings are used by the umbraco integrations

##### Configure an absolute or relative url to redirect successful or erronous logins to.
Can also be configured on a per-login basis using the event callbacks of IcelandAuthController
```js
IcelandAuth:SuccessRedirect - "http://icelandauth.localhost/page?=error=true"
IcelandAuth:ErrorRedirect
```

# .Net 5
When targetting .Net 5 we make use of X509ChainTrustMode.CustomRootTrust to build the certificate chain.

This simplifies setup by removing the requirement to install certificates locally to a certificate store. They are instead included with the package.

# Contribution

Building now requires VS 2019 Preview as Net5 is one of it's targets.

Looking to contribute something? Pull requests are welcome!

* More unit tests
* Documentation in icelandic
* Drop url rewrite module in umbraco samples and instead add route and mvc controller with redirect that keeps post body

# License

Vettvangur.IcelandAuth is licensed under the MIT license. (http://opensource.org/licenses/MIT)

# Other

Need help? Something on your mind? Drop us a line at [icelandauth@vettvangur.is](mailto:icelandauth@vettvangur.is)
