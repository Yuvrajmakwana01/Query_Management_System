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
using Repository.Services;

namespace MVC.Controllers
{
    // [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserInterface _repo;
        private readonly RedisServices _redis;

        private readonly EmailServices _emailService;

        public UserController(ILogger<UserController> logger, IUserInterface userRepository, RedisServices redis, EmailServices emailService)
        {
            _logger = logger;
            _repo = userRepository;
            _redis = redis;
            _emailService = emailService;
        }

        // public IActionResult Index()
        // {
        //     return View();
        // }
        public IActionResult Register()
        {
            return View();
        }

        // [HttpPost]
        // public IActionResult Register(t_Registration user)
        // {
        //     _logger.LogInformation("Incoming Role: {Role}", user.c_Role);

        //     if (!ModelState.IsValid)
        //     {
        //         return View(user);
        //     }

        //     if (string.IsNullOrWhiteSpace(user.c_Role)|| user.c_Role=="Default")
        //     {
        //         user.c_Role = "User"; 
        //     }

        //     bool result = _repo.Register(user);

        //     if (result)
        //     {
        //         TempData["msg"] = "Registration Successful !!!";
        //     }
        //     else
        //     {
        //         TempData["msg"] = "Problem while registering user !!";
        //     }

        //     return RedirectToAction("Login","Account");
        // }


        // [HttpPost]
        // public async Task<IActionResult> Register(t_Registration user)
        // {
        //     _logger.LogInformation("Incoming Role: {Role}", user.c_Role);

        //     if (!ModelState.IsValid)
        //     {
        //         return View(user);
        //     }

        //     if (string.IsNullOrWhiteSpace(user.c_Role) || user.c_Role == "Default")
        //     {
        //         user.c_Role = "User";
        //     }

        //     bool result = await _repo.Register(user);

        //     if (result)
        //     {
        //         string registeredRole = string.Equals(user.c_Role, "Employee", StringComparison.OrdinalIgnoreCase) ? "Employee"
        //             : "User";

        //         await _redis.AddNotification(
        //             "admin",
        //             registeredRole == "Employee" ? $" New Employee Joined: {user.c_EmpName} | {DateTime.Now:dd MMM yyyy hh:mm tt}"
        //                 : $" New User Registered: {user.c_EmpName} | {DateTime.Now:dd MMM yyyy hh:mm tt}"
        //         );
        //         TempData["msg"] = "Registration Successful !!!";
        //     }
        //     else
        //     {
        //         TempData["msg"] = "Problem while registering user !!";
        //     }

        //     return RedirectToAction("Login", "Account");
        // }


        [HttpPost]
        public async Task<IActionResult> Register(t_Registration user)
        {
            _logger.LogInformation("Incoming Role: {Role}", user.c_Role);

            if (!ModelState.IsValid)
                return View(user);

            // default role
            if (string.IsNullOrWhiteSpace(user.c_Role) || user.c_Role == "Default")
                user.c_Role = "User";

            // async call
            bool result = await _repo.Register(user);

            if (result)
            {
                TempData["msg"] = "Registration Successful !!!";


                try
                {
                    // 1. Load Email Template
                    string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "WelcomeEmail.html");
                    string template = await System.IO.File.ReadAllTextAsync(templatePath);

                    string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "QueryLogo.png");

                    string baseUrl = $"{Request.Scheme}://{Request.Host}";
                    string loginUrl = $"{baseUrl}/Account/Login";

                    template = template.Replace("{{Name}}", user.c_EmpName ?? "User");
                    template = template.Replace("{{Email}}", user.c_EmailId ?? "");
                    template = template.Replace("{{LoginLink}}", loginUrl);

                    await _emailService.SendEmailWithLogo(
                        user.c_EmailId,
                        "Welcome to Our App 🎉",
                        template,
                        logoPath   // ✅ IMPORTANT
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError("Email failed: " + ex.Message);
                }
                string role = user.c_Role.Equals("Employee", StringComparison.OrdinalIgnoreCase)
                    ? "Employee"
                    : "User";

                // ✅ SINGLE STANDARD FORMAT
                string notifMessage =
                    $"New {role} Registered: {user.c_EmpName} ({user.c_EmailId})||{DateTime.Now:dd MMM yyyy hh:mm tt}";

                await _redis.AddNotification("admin", notifMessage);
            }
            else
            {
                TempData["msg"] = "Problem while registering user !!";
            }

            return RedirectToAction("Login", "Account");
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Repository.Interfaces;
// using Repository.Models;
// using Repository.Implementations;

// namespace MVC.Controllers
// {
//     public class UserController : Controller
//     {
//         private readonly ILogger<UserController> _logger;
//         private readonly IUserInterface _repo;
//         private readonly RedisServices _redis;

//         public UserController(
//             ILogger<UserController> logger,
//             IUserInterface userRepository,
//             RedisServices redis)
//         {
//             _logger = logger;
//             _repo = userRepository;
//             _redis = redis;
//         }

//         // public IActionResult Index()
//         // {
//         //     return View();
//         // }

//         [HttpGet]
//         public IActionResult Register()
//         {
//             return View();
//         }

//         [HttpPost]
//         public async Task<IActionResult> Register(t_Registration user)
//         {
//             _logger.LogInformation("Incoming Role: {Role}", user.c_Role);

//             if (!ModelState.IsValid)
//                 return View(user);

//             // default role
//             if (string.IsNullOrWhiteSpace(user.c_Role) || user.c_Role == "Default")
//                 user.c_Role = "User";

//             // async call
//             bool result = await _repo.Register(user);

//             if (result)
//             {
//                 TempData["msg"] = "Registration Successful !!!";

//                 string role = user.c_Role.Equals("Employee", StringComparison.OrdinalIgnoreCase)
//                     ? "Employee"
//                     : "User";

//                 // ✅ SINGLE STANDARD FORMAT
//                 string notifMessage =
//                     $"New {role} Registered: {user.c_EmpName} ({user.c_EmailId})||{DateTime.Now:dd MMM yyyy hh:mm tt}";

//                 await _redis.AddNotification("admin", notifMessage);
//             }
//             else
//             {
//                 TempData["msg"] = "Problem while registering user !!";
//             }

//             return RedirectToAction("Login", "Account");
//         }
//     }
// }