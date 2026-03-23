namespace MVC.Controllers;

using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models;
using Repository.Services;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class QueryController : Controller
{
    private readonly IQueryInterface _queryRepository;

    private readonly ElasticSearchServices _elastic;

    private readonly RedisServices _redis;
    private readonly RabbitService _rabbit;

    public QueryController(IQueryInterface queryRepository, ElasticSearchServices elastic, RedisServices redis, RabbitService rabbit)
    {
        _queryRepository = queryRepository;
        _elastic = elastic;
        _redis = redis;
        _rabbit = rabbit;
    }

    // Page load
    public IActionResult QueryDashboard()
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



    // Get user queries
    public async Task<IActionResult> GetUserQueries()
    {
        int userid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

        var queries = await _queryRepository.GetUserQueries(userid);

        return Json(queries);
    }

    // Create Query
    // [HttpPost]
    // public async Task<IActionResult> CreateQuery(t_Query query)
    // {
    //     query.c_UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

    //     await _queryRepository.AddQuery(query);

    //     return Json(new { success = true });
    // }

    //  [HttpPost]
    // public async Task<IActionResult> CreateQuery(t_Query query)
    // {
    //     query.c_UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

    //     int newId = await _queryRepository.AddQuery(query);

    //     var savedQuery = await _queryRepository.GetQueryById(newId);

    //     if (savedQuery == null)
    //     {
    //         Console.WriteLine("❌ Query fetch failed");
    //         return Json(new { success = false });
    //     }

    //     await _elastic.IndexQueryAsync(savedQuery);

    //     return Json(new { success = true });
    // }


    [HttpPost]
    public async Task<IActionResult> CreateQuery(t_Query query)
    {
        query.c_UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

        bool success = await _queryRepository.AddQuery(query);

        if (success)
        {
            string msg = $"New Query Created: {query.c_Title}";
        }
        return Json(new { success = true });
    }

    // Get query by id
    public async Task<IActionResult> GetQueryById(int id)
    {
        var query = await _queryRepository.GetQueryById(id);

        return Json(query);
    }

    // Update query
    // [HttpPost]
    // public async Task<IActionResult> UpdateQuery(t_Query query)
    // {
    //     await _queryRepository.UpdateQuery(query);

    //     return Json(new { success = true });
    // }

    [HttpPost]
    public async Task<IActionResult> UpdateQuery(t_Query query)
    {
        await _queryRepository.UpdateQuery(query);

        // 🔥 Fetch latest data (optional but safer)
        var updatedQuery = await _queryRepository.GetQueryById(query.c_QueryId);

        await _elastic.IndexQueryAsync(updatedQuery);

        return Json(new { success = true });
    }

    // Delete query
    [HttpPost]
    public async Task<IActionResult> DeleteQuery(int id)
    {
        await _queryRepository.DeleteQuery(id);

        return Json(new { success = true });
    }

    // [HttpGet]
    // public async Task<IActionResult> Search(string keyword)
    // {
    //     var result = await _elastic.SearchQueryAsync(keyword);
    //     return Json(result);
    // }

    public async Task<IActionResult> Search(string keyword)
    {
        // If empty keyword — return normal user queries from DB (not elastic)
        if (string.IsNullOrWhiteSpace(keyword))
        {
            int uid = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            var all = await _queryRepository.GetUserQueries(uid);
            return Json(all);
        }
 
        int userId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
        var result = await _elastic.SearchQueryAsync(keyword, userId);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationCount()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized();

        var count = await _redis.GetNotificationCount(userId.Value.ToString());
        return Json(count);
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized();

        var list = await _redis.GetNotifications(userId.Value.ToString());
        return Json(list);
    }

    [HttpPost]
    public async Task<IActionResult> ClearNotifications()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized();

        await _redis.ClearAllNotifications(userId.Value.ToString());
        await _rabbit.ClearQueue();
        return Ok();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete(".AspNetCore.Session");
        return RedirectToAction("Login", "Account");
    }
}