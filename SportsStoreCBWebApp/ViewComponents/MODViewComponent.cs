using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SportsStoreCBWebApp.ViewComponents
{
  public class MODViewComponent : ViewComponent
  {
    private IConfiguration Configuration { get; }

    public MODViewComponent(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public Task<IViewComponentResult> InvokeAsync()
    {
      var result = Configuration["MOD"];
      return Task.FromResult<IViewComponentResult>(View("Default", result));
    }
  }
}
