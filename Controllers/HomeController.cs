using ActiveLights_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ActiveLights_MVC.Controllers
{
    // Denne controller bruges ikke
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("AllLamps");
        }
    }
}