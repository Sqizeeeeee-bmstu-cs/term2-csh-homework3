using homework3.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework3.Controllers;

/// <summary>
/// REST API контроллер для отчётов
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить полный отчёт со всеми тремя разделами
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetReport()
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

        return Ok(new
        {
            section1 = report1,
            section2 = report2,
            section3 = report3
        });
    }

    /// <summary>
    /// Получить только раздел 1 (полный список)
    /// </summary>
    [HttpGet("section1")]
    public async Task<ActionResult<IEnumerable<object>>> GetSection1()
    {
        var report = await _context.Professors
            .Include(p => p.Department)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Name,
                DepartmentName = p.Department!.Name,
                p.Publications
            })
            .ToListAsync();
        return Ok(report);
    }

    /// <summary>
    /// Получить только раздел 2 (количество по кафедрам)
    /// </summary>
    [HttpGet("section2")]
    public async Task<ActionResult<IEnumerable<object>>> GetSection2()
    {
        var report = await _context.Professors
            .GroupBy(p => p.Department!.Name)
            .Select(g => new
            {
                Department = g.Key,
                Count = g.Count()
            })
            .OrderBy(r => r.Department)
            .ToListAsync();
        return Ok(report);
    }

    /// <summary>
    /// Получить только раздел 3 (среднее количество публикаций)
    /// </summary>
    [HttpGet("section3")]
    public async Task<ActionResult<IEnumerable<object>>> GetSection3()
    {
        var report = await _context.Professors
            .GroupBy(p => p.Department!.Name)
            .Select(g => new
            {
                Department = g.Key,
                AvgPublications = g.Average(p => p.Publications)
            })
            .OrderByDescending(r => r.AvgPublications)
            .ToListAsync();
        return Ok(report);
    }
}
