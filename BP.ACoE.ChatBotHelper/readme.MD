# Chat Bot Helper package v1.0

## Salesforce Certificate Helper
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

## Salesforce Auth Service

Inject ISalesForceAuthService into startup as like 
requires IAppInsightsService, and ILogger from serilog configured and ``` SalesForceCertificate``` and ```AzureKeyVaultCertificate```
```
services.AddHttpClient<ISalesForceAuthService, SalesForceAuthService>();

```

## Azure Auth Service

to use this azure auth service which performs azure oauth2 
requires IAppInsightsService, and ILogger from serilog configured
Inject the service like

```
services.AddHttpClient<IAzureAuthService, AzureAuthService>();
```

## Azure Table Storage Service
Inject this service to use the azure storage tables in the application 

depends on serilog logger and configuration items as listed below

```
Uri  azure storage uri,  
Account Name:  storage account name
Key:  Storage account key
services.AddSingleton<IStorageService, AzureTableStorageService>( new TableServiceClient(new Uri(""), new TableSharedKeyCredential(accountName: "", accountKey: "")))
```

## Azure CosmosDB Service
Inject this service to use the azure cosmosdb in the application as storage 

in appsetting.json file

```
{
    "CosmosDb":{
        "HostUrl": "https://{db-account-name}.documents.azure.com:443/",
        "DatabaseName": "{Database Name}",
        "ContainerName": "{Container name}",
        "CosmosAccountKey":"{Account Key}"
    }
}
```

```
// in Startup.cs
services.Configure<AzureCosmosDbSettings>(configuration.GetSection(AzureCosmosDbSettings.AzureCosmosSettingName));
services.AddSingleton<IStorageService>(InitializeCosmosClientInstanceAsync().GetAwaiter().GetResult());


///----
private async Task<IStorageService> InitializeCosmosClientInstanceAsync()
    {
        var cosmosDbSettings = serviceProvider.GetRequiredService<IOptions<AzureCosmosDbSettings>>().Value;

        
        var databaseName = cosmosDbSettings.DatabaseName;
        var containerName = cosmosDbSettings.ContainerName;
        var account = cosmosDbSettings.HostUrl;
        var key = cosmosDbSettings.CosmosAccountKey;
        Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
        IStorageService cosmosDbService = new AzureStorageService(client, databaseName, containerName);
        Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
        await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

        return cosmosDbService;
    }

```

## Azure App Insights Service
Inject this service in startup.cs

```
services.AddSingleton<TelemetryClient>(new TelemetryClient(new TelemetryConfiguration(Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"))));

services.AddScoped<IAppInsightsService, AppInsightsService>();

```

## Salesforce LiveAgent Service

This service contains the methods to connect with Salesforce Live Agent API.
Configuration settings 
```
    {
        "SalesForceLiveAgentSettings": {
            "BuildAvailableLiveAgentUrl": true, // this will determine if buttonId, DeploymentId, OrgId need to be injected in the urls default is true
            "OrganizationId": "{value for SF Org id}",
            "DeploymentId":"{value for SF Deployment id}",
            "ButtonId":"{value for SF Button id}",
            "AvailableLiveAgentsUrl":"",
            "SendChatMessageUrl":"",
            "GetChatMessagesUrl":"",
            "StartChatUrl":"",
            "LiveAgentSessionUrl":"",
            "LiveAgentApiVersion":"{Live Agent API version to be used}",
        }
    }
```
Inject the service in startup.cs like
```


services.Configure<SalesForceLiveAgentSettings>(configuration.GetSection());

// Depends on HttpClient, Serilog Logger, IOptions<SalesForceLiveAgentSettings> and IAppInsightsService service

services.AddHttpClient<ISalesForceLiveAgentService, SalesForceLiveAgentService>();

```

## Exception Middleware

Use this extension helper to handle exceptions globally.

```
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.ConfigureExceptionHandler(logger);
}

```

## JSON Helper

Use the extension method ```.ToJson()``` to get serialized version of any object

## Object To Dictionary Helper

```
    {object}.ToDictionary()
```
user this extension mehtod to convert any object to dictionary.


## Email Service

This service contains the methods to send email.
Configuration settings 
```
    {
       "GraphApiAuthSettings": {
            "Instance": "https://login.microsoftonline.com/{0}/oauth2/v2.0/token",
            "Tenant": "{value for Tenant}",
            "ClientId": "{value for ClientId}",
            "ClientSecret": "{value for SecretId}"
        }
    }
```
Inject the service in startup.cs like
```

 services.AddOptions<GraphApiAuthSettings>()
                .Bind(Configuration.GetSection("GraphApiAuthSettings"))
                .ValidateDataAnnotations();

// Depends on IConfiguration, Serilog Logger, IOptions<GraphApiAuthOptions> and IAzureAuthService service

 services.AddScoped<IEmailService, EmailService>();

```

## ChatTransaction Service

This service contains the methods related to get transaction by conversationId.
```
    {
       "ChatTransactionSettings": {
            "ChatTxTable": "{value for ChatTxTable}",
            "PartitionKey": "{value for PartitionKey}"
        }
    }
```
Inject the service in startup.cs like
```

// Depends on IConfiguration configuration, ILogger logger, IStorageService storageService

services.Configure<ChatTransactionSettings>(configuration.GetSection("ChatTransactionSettings"));
services.AddScoped<IChatTransactionService, ChatTransactionService>();

```

## ChatTranscript Service

This service contains the methods related to chat transcripts.
```
    {
        "ChatTranscriptSettings": {
            "ChatbotName": "{value for ChatbotName}",
            "SendTranscriptTable": {value for SendTranscriptTable},
            "PartitionKey": {value for PartitionKey},
            "RewardEmailTemplatePath": {value for RewardEmail Template Path},
            "StationEmailTemplatePath": {value for StationEmail Template Path},
            "GeneralFleetEmailTemplatePath": {value for GeneralFleetEmail Template Path},
            "FleetEmailTemplatePath": {value for FleetEmail Template Path},
            "CustomerSupportEmailTemplatePath": {value for CustomerSupportEmail Template Path},
            "ChatTranscriptPDFHeaderImagePath": {value for ChatTranscriptPDFHeaderImagePath},
            "ChatTranscriptPDFFooterImagePath": {value for ChatTranscriptPDFFooterImagePath},
            "TimeZone": {value for Time Zone},
            "EmailFromAddress": {value for Email From Address},
            "TestEnvironment": {value for Environment},
            "SendCCEmail": {value for SendCCEmail},
            "EmailCCAddress": {value for EmailCCAddress},
            "ChatBotTranscriptName": {value for ChatBot Transcript Name},
            "EmailTemplatePath": {value for Email Template Path},
            "FLEET_FORMToEmail": {value for FLEET_FORMToEmail},
            "REWARD_FORMToEmail": {value for REWARD_FORMToEmail},
            "PAYMENT_FORMToEmail": {value for PAYMENT_FORMToEmail},
            "APP_TECH_FORMToEmail": {value for APP_TECH_FORMToEmail},
            "CHANGE_EMAIL_FORMToEmail": {value for CHANGE_EMAIL_FORMToEmail},
            "CLOSE_ACCOUNT_FORMToEmail": {value for CLOSE_ACCOUNT_FORMToEmail},
            "STATION_ISSUE_FORMToEmail": {value for STATION_ISSUE_FORMToEmail},
            "CLICKCOLLECT_RELATEDToEmail": {value for CLICKCOLLECT_RELATEDToEmail},
            "ACCOUNT_RELATEDToEmail": {value for ACCOUNT_RELATEDToEmail},
            "PAYMENT_RELATEDToEmail": {value for PAYMENT_RELATEDToEmail},
            "REWARDS_RELATEDToEmail": {value for REWARDS_RELATEDToEmail},
            "STATION_RELATEDToEmail": {value for STATION_RELATEDToEmail},
            "APP_RELATEDToEmail": {value for APP_RELATEDToEmail},
            "OTHER_QUERYToEmail": {value for OTHER_QUERYToEmail},
            "FLEET_RELATEDToEmail": {value for FLEET_RELATEDToEmail}
  }
    }
```
Inject the service in startup.cs like
```

// Depends on IConfiguration, Serilog Logger,HttpClient client, IStorageService storageService, IEncryptionService decryption,
IEmailService emailService,IChatTransactionService chatTransactionService, ITranscriptStore transcriptStore

services.Configure<ChatTranscriptSettings>(configuration.GetSection("ChatTranscriptSettings"));
services.AddScoped<IChatTranscriptService, ChatTranscriptService>();

```

## Encryption Service

This service contains the methods related to encryption and decryption.
```
    {
       "EncryptionSettings": {
            "EncryptionKey": "{value for EncryptionKey}"
  }
    }
```
Inject the service in startup.cs like
```

// Depends on IConfiguration, Serilog Logger

services.Configure<EncryptionSettings>(configuration.GetSection("EncryptionSettings"));
services.AddScoped<IEncryptionService, EncryptionService>();

```