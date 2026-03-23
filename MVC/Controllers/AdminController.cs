using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.Interfaces;
using Repository.Models;
using Repository.Services;

namespace MVC.Controllers
{
    // [Route("[controller]")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminController : Controller
    {
        // private readonly ILogger<AdminController> _logger;
        private readonly IAdminInterface _adminRepo;

        private readonly ElasticSearchServices _elastic;

        private readonly RedisServices _redis;

        private readonly RabbitService _rabbit;


        public AdminController(ILogger<AdminController> logger,
        IAdminInterface adminRepo,
         ElasticSearchServices elastic,
         RedisServices redis,
         RabbitService rabbit)
        {
            // _logger = logger;
            _adminRepo = adminRepo;
            _elastic = elastic;
            _redis = redis;
            _rabbit = rabbit;
        }


        // [HttpGet]
        // public async Task<IActionResult> Search(string title, string status)
        // {
        //     var result = await _elastic.Search(title, status);
        //     return View(result.Documents);
        // }

        // [HttpGet]
        // public async Task<IActionResult> SearchJson(string title = "", string status = "")
        // {
        //     var result = await _elastic.Search(title ?? "", status ?? "");
        //     return Json(result.Documents);
        // }
        public async Task<IActionResult> GetNotificationCount()
        {
            var count = await _redis.GetNotificationCount("admin");
            return Json(count);
        }


        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var list = await _redis.GetNotifications("admin");
            return Json(list);
        }

        [HttpPost]
        public async Task<IActionResult> ClearNotifications()
        {
            await _redis.ClearAllNotifications("admin"); // Redis Clear
            await _rabbit.ClearQueue();    // RabbitMQ Clear
            return Ok();
        }


        // [HttpGet]
        // public async Task<IActionResult> SearchQueries(string keyword, string status, DateTime? fromDate, DateTime? toDate)
        // {
        //     var result = await _elastic.AdminSearchAsync(keyword, status, fromDate, toDate);
        //     return Json(result);
        // }


        public async Task<IActionResult> SearchQueries(
            string keyword,
            string status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // If nothing is provided, return all queries from DB
            bool hasAnyFilter = !string.IsNullOrWhiteSpace(keyword)
                             || !string.IsNullOrWhiteSpace(status)
                             || fromDate.HasValue
                             || toDate.HasValue;

            if (!hasAnyFilter)
            {
                var all = await _adminRepo.GetAllQuery();
                return Json(all);
            }

            var result = await _elastic.AdminSearchAsync(keyword, status, fromDate, toDate);
            return Json(result);
        }




        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Query()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return RedirectToAction("Index", "Home");
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
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return RedirectToAction("Index", "Home");
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

        [HttpPost]
        public async Task<IActionResult> RemoveNotification(string message)
        {
            await _redis.RemoveNotification("admin", message);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Employee()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployeeData()
        {
            var data = await _adminRepo.GetAllEmployee();
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                await _adminRepo.DeleteEmployee(id);
                return Json(new { success = true, message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting employee: " + ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}