using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SportsStoreCBWebApp
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => 
            {
              webBuilder.ConfigureAppConfiguration((hostingContext, config) => {
                  SetupKeyVault(hostingContext, config);
              })
              .UseStartup<Startup>();
            });

    private static void SetupKeyVault(WebHostBuilderContext hostingContext, IConfigurationBuilder config)
    {
      var buildConfig = config.Build();
      var keyVaultEndpoint = buildConfig["SSKeyVault"];
      if (!string.IsNullOrEmpty(keyVaultEndpoint))
      {
        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        config.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
      }
    }
  }
}
