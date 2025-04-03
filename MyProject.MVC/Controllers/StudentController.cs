using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyProject.MVC.Controllers
{

    public class StudentController : Controller
    {
        private readonly ILogger<StudentController> _logger;

        public StudentController(ILogger<StudentController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult EditProfile()
        {
            return View();
        }

        public IActionResult Mentoring()
        {
            return View();
        }

        public IActionResult Feedback()
        {
            return View();
        }

        public IActionResult StudentMaterial()
        {
            return View();
        }

        public IActionResult TimeTable()
        {
            return View();
        }

    }
}