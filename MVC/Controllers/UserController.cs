using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.Implementations;
using Repository.Models;
using Repository.Interfaces;

namespace MVC.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserInterface _repo;

        public UserController(ILogger<UserController> logger, IUserInterface userRepository)
        {
            _logger = logger;
            _repo= userRepository;
        }

        // public IActionResult Index()
        // {
        //     return View();
        // }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(t_Registration user)
        {
            _logger.LogInformation("Incoming Role: {Role}", user.c_Role);

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (string.IsNullOrWhiteSpace(user.c_Role)|| user.c_Role=="Default")
            {
                user.c_Role = "User"; 
            }

            bool result = _repo.Register(user);

            if (result)
            {
                TempData["msg"] = "Registration Successful !!!";
            }
            else
            {
                TempData["msg"] = "Problem while registering user !!";
            }

            return RedirectToAction("Login","Account");
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}