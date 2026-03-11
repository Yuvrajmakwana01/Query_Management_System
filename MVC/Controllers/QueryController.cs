namespace MVC.Controllers;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models;

public class QueryController : Controller
{
    private readonly IQueryInterface _queryRepository;

    public QueryController(IQueryInterface queryRepository)
    {
        _queryRepository = queryRepository;
    }

    // Page load
    public IActionResult QueryDashboard()
    {
        return View();
    }

    
    public IActionResult Dashboard()
    {
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
    [HttpPost]
    public async Task<IActionResult> CreateQuery(t_Query query)
    {
        query.c_UserId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

        await _queryRepository.AddQuery(query);

        return Json(new { success = true });
    }

    // Get query by id
    public async Task<IActionResult> GetQueryById(int id)
    {
        var query = await _queryRepository.GetQueryById(id);

        return Json(query);
    }

    // Update query
    [HttpPost]
    public async Task<IActionResult> UpdateQuery(t_Query query)
    {
        await _queryRepository.UpdateQuery(query);

        return Json(new { success = true });
    }

    // Delete query
    [HttpPost]
    public async Task<IActionResult> DeleteQuery(int id)
    {
        await _queryRepository.DeleteQuery(id);

        return Json(new { success = true });
    }
}