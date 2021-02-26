using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsStoreCBWebApp.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index() => View();
    public ActionResult About() => View();
    public ActionResult Contact() => View();
  }
}
