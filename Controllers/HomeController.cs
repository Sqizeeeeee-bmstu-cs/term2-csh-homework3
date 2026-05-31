using Microsoft.AspNetCore.Mvc;

namespace homework3.Controllers;

/// <summary>
/// Контроллер для обслуживания главной страницы
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Возвращает главную страницу (index.html из wwwroot)
    /// </summary>
    public IActionResult Index()
    {
        return File("~/index.html", "text/html");
    }
}
