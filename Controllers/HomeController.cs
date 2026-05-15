using Microsoft.AspNetCore.Mvc;

namespace homework3.Controllers;

/// <summary>
/// Главный контроллер
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Главная страница
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }
}
