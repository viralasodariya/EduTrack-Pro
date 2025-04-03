using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyProject.MVC.Controllers
{

    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Student()
        {
            return View();
        }

        public IActionResult Class()
        {
            return View();
        }

        public IActionResult Subject()
        {
            return View();
        }

        public IActionResult Notification()
        {
            return View();
        }

        public IActionResult Progress()
        {
            return View();
        }
        public IActionResult TimeTable()
        {
            return View();
        }

    }
}