using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vettvangur.IcelandAuth.Sample.AspNetCore.Models;

namespace Vettvangur.IcelandAuth.Sample.AspNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            SamlLogin login = null;

            if (Request.Method == "POST" && Request.Form.ContainsKey("token"))
            {
                var authSvc = new IcelandAuthService(_configuration, _logger);
                login = authSvc.VerifySaml(Request.Form["token"], HttpContext.Connection.RemoteIpAddress.ToString());
            }

            return View(login);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
