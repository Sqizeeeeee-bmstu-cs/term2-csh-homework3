using homework3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework3.Controllers;

/// <summary>
/// Контроллер для отчётов
/// </summary>
public class ReportsController : Controller
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Главная страница отчётов со всеми тремя разделами
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Раздел 1: Полный список преподавателей с названием кафедры
        var report1 = await _context.Professors
            .Include(p => p.Department)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Name,
                DepartmentName = p.Department!.Name,
                p.Publications
            })
            .ToListAsync();

        // Раздел 2: Количество преподавателей по кафедрам
        var report2 = await _context.Professors
            .GroupBy(p => p.Department!.Name)
            .Select(g => new
            {
                Department = g.Key,
                Count = g.Count()
            })
            .OrderBy(r => r.Department)
            .ToListAsync();

        // Раздел 3: Среднее количество публикаций по кафедрам
        var report3 = await _context.Professors
            .GroupBy(p => p.Department!.Name)
            .Select(g => new
            {
                Department = g.Key,
                AvgPublications = g.Average(p => p.Publications)
            })
            .OrderByDescending(r => r.AvgPublications)
            .ToListAsync();

        ViewBag.Report1 = report1;
        ViewBag.Report2 = report2;
        ViewBag.Report3 = report3;

        return View();
    }
}
