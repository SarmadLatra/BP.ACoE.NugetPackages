# Chat bot package usage

1. using Salesforce Certificate Helper
2. Provide the config for salesforce as the following section
```
{
    // please make sure the section and key names are matched
    // will be used to save the certificate on disk
    "SalesForceCertificate":{
          "SalesForceCertPath": "{certificate path on server}",
          "SalesForceCertName": "{certificate-name}.pfx",
          "SalesForceCertPassword": "{password to open the certificate}"
    },
    // following section will be used to read the certificate from azure keyvault

    "AzureKeyVaultCertificate":{
        "KeyVaultCertUri": "{URL to Certificate resource in keyvault}",
        "AzureAuthUrl": "https://login.microsoftonline.com/{TenantID}/oauth2/token",
        "AzureKeyVaultScope": "https://vault.azure.net/.default"
        "AppClientId": "{Azure AD Client ID to access keyvault}",
        "AppClientSecret": "{Azure AD Client Secret to access keyvault}",
        "CertVersion":"{Certificate Azure Keyvault version}" // default to '?api-version=7.0'
    }
}

// Put these vaules in keyvault secrets/keys as    {section}__{keyname}
// e.g:  AzureKeyVaultCertificate__KeyVaultCertUri 
```
3. Use in the application bootstrap section e.g. startup.cs
```
service.AddSalesForceCertificate()
```
