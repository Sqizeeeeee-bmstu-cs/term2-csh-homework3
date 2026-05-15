using homework3.Data;
using homework3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework3.Controllers;

/// <summary>
/// Контроллер для управления преподавателями
/// </summary>
public class ProfessorsController : Controller
{
    private readonly AppDbContext _context;

    public ProfessorsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отображение списка всех преподавателей (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var professors = await _context.Professors
            .Include(p => p.Department)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View(professors);
    }

    /// <summary>
    /// Форма создания нового преподавателя (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
        ViewBag.Departments = departments;
        return View();
    }

    /// <summary>
    /// Сохранение нового преподавателя (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DepartmentId,Name,Publications")] Professor professor)
    {
        if (string.IsNullOrWhiteSpace(professor.Name))
        {
            ModelState.AddModelError("Name", "Имя преподавателя не может быть пустым");
        }

        if (professor.Publications < 0)
        {
            ModelState.AddModelError("Publications", "Количество публикаций не может быть отрицательным");
        }

        if (professor.DepartmentId <= 0)
        {
            ModelState.AddModelError("DepartmentId", "Выберите кафедру");
        }

        if (ModelState.IsValid)
        {
            _context.Add(professor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        var departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
        ViewBag.Departments = departments;
        return View(professor);
    }

    /// <summary>
    /// Форма редактирования преподавателя (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var professor = await _context.Professors.FindAsync(id);
        if (professor == null)
            return NotFound();

        var departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
        ViewBag.Departments = departments;
        return View(professor);
    }

    /// <summary>
    /// Сохранение изменений преподавателя (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,DepartmentId,Name,Publications")] Professor professor)
    {
        if (id != professor.Id)
            return NotFound();

        if (string.IsNullOrWhiteSpace(professor.Name))
        {
            ModelState.AddModelError("Name", "Имя преподавателя не может быть пустым");
        }

        if (professor.Publications < 0)
        {
            ModelState.AddModelError("Publications", "Количество публикаций не может быть отрицательным");
        }

        if (professor.DepartmentId <= 0)
        {
            ModelState.AddModelError("DepartmentId", "Выберите кафедру");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(professor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfessorExists(professor.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        var departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
        ViewBag.Departments = departments;
        return View(professor);
    }

    /// <summary>
    /// Форма подтверждения удаления преподавателя (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var professor = await _context.Professors
            .Include(p => p.Department)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (professor == null)
            return NotFound();

        return View(professor);
    }

    /// <summary>
    /// Удаление преподавателя (POST)
    /// </summary>
    [HttpPost("Professors/DeleteConfirmed/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var professor = await _context.Professors.FindAsync(id);
        if (professor != null)
        {
            _context.Professors.Remove(professor);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool ProfessorExists(int id)
    {
        return _context.Professors.Any(e => e.Id == id);
    }
}
