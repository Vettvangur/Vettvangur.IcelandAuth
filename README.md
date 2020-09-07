# Vettvangur.IcelandAuth <sup>v1.0.0</sup>

Vettvangur.IcelandAuth is an open-source .Net project intended to simplify integrating with the island.is authentication service.

island.is's authentication service allows for authenticating icelandic nationals using digital certificates / Íslykill.

A prior contract with island.is is needed before usage, see https://island.is/#permit-884. General information on the island.is authentication service can be found [here](https://island.is/um-island-is/innskraningarthjonustan/).

This project was developed according to fixes and suggestions from [Syndis](https://www.syndis.is/) and influenced in part by the work done [here](https://github.com/digitaliceland/innskraningar-daemi/).

# Getting Started

## Requirements
At least .Net Framework 4.6.1 or .Net Core 2.

The NetStandard library (NetStandard essentially just contains mappings) and some Microsoft.Extensions packages will be installed if missing. They have a light footprint and are used for abstractions made available in .Net Standard.

## Signature verification

To be able to verify the signature returned from island.is you will need to install the audkenni certificate chain, http://www.audkenni.is/adstod/skilrikjakedjur.cfm.

The certificate Auðkennisrót needs to be installed into trusted roots in the server hosts certificate store.

The intermediate certificates should be added to Intermediate Certification Authorities.

Installing root certificates is outside the scope of this documentation but a detailed step-by-step can for example be found [here](https://docs.microsoft.com/en-us/skype-sdk/sdn/articles/installing-the-trusted-root-certificate).

## Configuration

The sample projects in this repository show how to integrate IcelandAuth with an Umbraco site. Those samples use the core IcelandAuth library and additional umbraco helpers from Vettvangur.IcelandAuth.Umbraco7[/8].

For .Net Core and custom integrations you can use the core library validation and inspect the ***Valid*** property of the returned SamlLogin object.

IcelandAuth is configured using appSettings key values, these are commonly stored in Web.config for Asp.Net and appSettings.json for Asp.Net Core projects.

The following documentation shows all configurable keys preceding with 'IcelandAuth.' as required in Web.config. Asp.Net Core appSettings configuration should instead be nested under the IcelandAuth section similar to the following:

### Asp.Net Core appSettings.json structure

```json
{
    "IcelandAuth": {
        "Audience": "",
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

##### Constrain audience - Required (Audience is sometimes referred to as island.is ID.)
```xml
IcelandAuth.Audience
```
##### Ensure SAML response url matches SAML response url destination. - Optional
```xml
IcelandAuth.Destination
```
##### The SSN used for contract with island.is. - Optional
```xml
IcelandAuth.DestinationSSN
```

### Configuration - Additional options

##### Constrain authentication method used on island.is.

Possible values include:

* "Rafræn skilríki"
* "Rafræn símaskilríki"
* "Íslykill"

Seperate multiple values with comma
```xml
IcelandAuth.Authentication
<!-- <add key="IcelandAuth.Authentication" value="Rafræn skilríki,Rafræn símaskilríki" /> -->
```

##### Check if the users IP matches the one seen at authentication.
Check if the users IP matches the one seen at authentication.
This usually fails during development, island.is will see the public ip the development machine communicates using while the development server, if hosted on your internal network, will see your intranet address.

Recommendation is to disable in development and enable live
```xml
IcelandAuth.VerifyIPAddress <!-- default True -->
```
##### Log the Saml response received from island.is
Take care to only enable this option in development!
```xml
IcelandAuth.LogSamlResponse <!-- default False -->
```

### Configuration - Umbraco packages

The following settings are used by the umbraco integrations

##### Configure an absolute or relative url to redirect successful or erronous logins to.
Can also be configured on a per-login basis using the event callbacks of IcelandAuthController
```xml
IcelandAuth.SuccessRedirect
IcelandAuth.ErrorRedirect
```

## Contribution

Looking to contribute something? Pull requests are welcome!

* .Net Core Sample is sorely missing!

## License

Vettvangur.IcelandAuth is licensed under the MIT license. (http://opensource.org/licenses/MIT)

## Other

Need help? Something on your mind? Drop us a line at [icelandauth@vettvangur.is](mailto:icelandauth@vettvangur.is)
