using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsStoreCBWebApp.Controllers
{
  public class HomeController : Controller
  {
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
      _configuration = configuration;
    }
    public async Task<IActionResult> Index()
    {
      AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
      KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
      var secrets = await keyVaultClient.GetSecretsAsync($"{_configuration["SSKeyVault"]}");
      Dictionary<string, string> secretValueList = new Dictionary<string, string>();
      foreach (var item in secrets)
      {
        var secret = await keyVaultClient.GetSecretAsync($"{item.Id}");
        secretValueList.Add(item.Id, secret.Value);
      }
      return View(secretValueList);
    }
    public ActionResult About() => View();
    public ActionResult Contact() => View();

    public ActionResult Throw()
    {
      throw new EntryPointNotFoundException("This is a user thrown Exception");
    }
  }
}
