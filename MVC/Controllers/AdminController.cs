using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.Interfaces;
using Repository.Models;

namespace MVC.Controllers
{
    // [Route("[controller]")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminController : Controller
    {
        // private readonly ILogger<AdminController> _logger;
        private readonly IAdminInterface _adminRepo;

        public AdminController(ILogger<AdminController> logger, IAdminInterface adminRepo)
        {
            // _logger = logger;
            _adminRepo = adminRepo;
        }

        public IActionResult Index()
        {
            if(HttpContext.Session.GetString("UserRole")==null)
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        public IActionResult Query()
        {
            if(HttpContext.Session.GetString("UserRole")==null)
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

    
    public async Task<IActionResult> GetDashboardData()
    {
        var data = await _adminRepo.GetAll();
        return Ok(data);
    }


        public async Task<IActionResult> GetAllQuery()
        {
            List<t_Query> queries = await _adminRepo.GetAllQuery();
            // Console.WriteLine(queries[0].c_EmpId);
            return Json(queries);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Debugging statement to log the received id
                Console.WriteLine("Received id: " + id);

                // Find the Query with the given id
                var query = await _adminRepo.GetOneQuery(id);

                if (query == null)
                {
                    // Query not found, return an error response
                    return Json(new { success = false, message = "Query not found." });
                }

                // Delete the Query
                await _adminRepo.Delete(query);

                return Json(new { success = true, message = "Query Deleted..." });
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the delete operation
                return Json(new { success = false, message = "An error occurred while deleting the Query." + ex.Message });
            }
        }

        public IActionResult GetAllUsersPage()
        {
            if(HttpContext.Session.GetString("UserRole")==null)
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsersData()
        {
            // if(HttpContext.Session.GetString("Role")==null)
            // {
            //     return RedirectToAction("Index","Home");
            // }
            var users = await _adminRepo.GetAllUsers();
            return Json(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // if(HttpContext.Session.GetString("Role")==null)
            // {
            //     return RedirectToAction("Index","Home");
            // }
            await _adminRepo.DeleteUser(id);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}