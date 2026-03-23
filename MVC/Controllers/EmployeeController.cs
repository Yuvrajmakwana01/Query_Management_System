using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.Interfaces;
using Repository.Services;

namespace MVC.Controllers
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class EmployeeController : Controller
    {

        private readonly IEmployeeInterface _repo;
        private readonly ElasticSearchServices _elastic;
        private readonly RedisServices _redis;
        private readonly RabbitService _rabbit;

        public EmployeeController(
            IEmployeeInterface repo,
            ElasticSearchServices elastic,
            RedisServices redis,
            RabbitService rabbit)
        {
            _repo = repo;
            _elastic = elastic;
            _redis = redis;
            _rabbit = rabbit;
        }


        public IActionResult EmpDashboard()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetUnsolvedQueries()
        {
            var data = await _repo.GetUnsolvedQueries();
            return Json(data);
        }


        [HttpGet]
        public async Task<JsonResult> GetPersonalMetrics()
        {
            int empId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Count how many queries this employee has solved
            int solvedCount = await _repo.GetResolvedCountByEmployee(empId);

            // Total unsolved queries currently in the system
            var allUnsolved = await _repo.GetUnsolvedQueries();
            int activeCount = allUnsolved.Count;

            // ADD THIS LINE: Filter the list you already fetched for High priority
            // Note: Use the exact property name from your Model (e.g., c_Priority or Priority)
            int urgentCount = allUnsolved.Count(q => q.c_Priority == "High");

            return Json(new
            {
                solvedCount = solvedCount,
                activeCount = activeCount,
                urgentCount = urgentCount
            });
        }

        [HttpPost]
        public async Task<JsonResult> UpdateQueryStatus(int c_QueryId, string c_Status, string c_Comment)
        {
            int? empId = HttpContext.Session.GetInt32("UserId");
            if (empId == null) return Json(new { success = false, message = "Session Expired" });

            var success = await _repo.ResolveQuery(c_QueryId, empId.Value, c_Status, c_Comment);
            return Json(new { success });
        }


        // [HttpPost]
        // public async Task<JsonResult> UpdateQueryStatus(int c_QueryId, string c_Status, string c_Comment)
        // {
        //     int? empId = HttpContext.Session.GetInt32("UserId");
        //     if (empId == null)
        //         return Json(new { success = false, message = "Session Expired" });

        //     // 1️⃣ Update DB
        //     var success = await _repo.ResolveQuery(c_QueryId, empId.Value, c_Status, c_Comment);

        //     if (success)
        //     {
        //         // 2️⃣ Get latest updated record
        //         var updatedQuery = await _repo.GetQueryById(c_QueryId);

        //         // 3️⃣  Update Elasticsearch
        //         await _elastic.IndexQueryAsync(updatedQuery);
        //     }

        //     return Json(new { success });
        // }


        // [HttpGet]
        // public async Task<JsonResult> GetPersonalAnalytics()
        // {
        //     int empId = HttpContext.Session.GetInt32("UserId") ?? 0;
        //     var metrics = await _repo.GetEmployeeDashboardMetrics(empId);

        //     return Json(new
        //     {
        //         solvedCount = metrics["solved"],
        //         activeCount = metrics["active"],
        //         urgentCount = metrics["urgent"]
        //     });
        // }

        [HttpGet]
        public async Task<IActionResult> Filter(DateTime? fromDate, DateTime? toDate)
        {
            var result = await _elastic.FilterByDate(fromDate, toDate);
            return Json(result);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public async Task<IActionResult> GetNotificationCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Redis se employee ke notifications count nikaalein
            var count = await _redis.GetNotificationCount(userId.Value.ToString());
            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Redis se list nikaalein
            var list = await _redis.GetNotifications(userId.Value.ToString());
            return Json(list);
        }

        [HttpPost]
        public async Task<IActionResult> ClearNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Redis se data clear karein aur RabbitMQ queue clean karein
            await _redis.ClearAllNotifications(userId.Value.ToString());

            // Note: Agar Employee ki queue alag hai toh specify karein, 
            // varna ClearQueue logic aapki service handle karegi
            await _rabbit.ClearQueue();

            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}