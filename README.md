# SportsStoreCBWebApp - DotNet Core 3.1 MVC Web App with Blob Storage and Cosmos Db

#### Packages

- Nuget Packages for the project
  - For Razor Page Runtime Compilation
    - Install-Package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation -Version 3.1.8
  - For Azure Storage
    - Install-Package WindowsAzure.Storage -Version 9.3.3
  - For Cosmos Db
    - Using .Net 3.0
      - Install-Package Microsoft.Azure.Cosmos -Version 3.16.0 for using CosmosClient as the class
  - For Azure Application Insights
    - Install-Package Microsoft.ApplicationInsights.AspNetCore -Version 2.17.0
    - Install-Package Microsoft.Extensions.Logging.ApplicationInsights -Version 2.17.0 (for logging ILogger user defined logs in ApplicationInsights)
  - For Azure KeyVault
    - Install-Package Microsoft.Azure.KeyVault -Version 3.0.5
    - Install-Package Microsoft.Azure.Services.AppAuthentication -Version 1.6.1
    - Install-Package Microsoft.Extensions.Configuration.AzureKeyVault -Version 3.1.12
  - For Redis Cache
    - Install-Package Microsoft.Extensions.Caching.Redis -Version 2.2.0

#### libman - lightweight client-side library tool

- Commands
  - dotnet tool install --global Microsoft.Web.LibraryManager.Cli --version 2.1.76 (for installing it globally)
  - libman --version
  - libman --help
  - libman init --default-destination wwwroot/lib --default-provider cdnjs(will create a libman.json file)
