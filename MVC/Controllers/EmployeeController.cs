using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repository.Interfaces;

namespace MVC.Controllers
{
    public class EmployeeController : Controller
    {

        private readonly IEmployeeInterface _repo;
        public EmployeeController(IEmployeeInterface repo) { _repo = repo; }


        public IActionResult EmpDashboard() => View();


        public IActionResult Dashboard() => View();

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


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}