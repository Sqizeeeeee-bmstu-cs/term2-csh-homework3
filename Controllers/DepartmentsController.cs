using homework3.Data;
using homework3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework3.Controllers;

/// <summary>
/// Контроллер для управления кафедрами
/// </summary>
public class DepartmentsController : Controller
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отображение списка всех кафедр (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
        return View(departments);
    }

    /// <summary>
    /// Форма создания новой кафедры (GET)
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Сохранение новой кафедры (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Department department)
    {
        if (string.IsNullOrWhiteSpace(department.Name))
        {
            ModelState.AddModelError("Name", "Название кафедры не может быть пустым");
        }

        if (ModelState.IsValid)
        {
            _context.Add(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    /// <summary>
    /// Форма редактирования кафедры (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound();

        return View(department);
    }

    /// <summary>
    /// Сохранение изменений кафедры (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Department department)
    {
        if (id != department.Id)
            return NotFound();

        if (string.IsNullOrWhiteSpace(department.Name))
        {
            ModelState.AddModelError("Name", "Название кафедры не может быть пустым");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(department.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    /// <summary>
    /// Форма подтверждения удаления кафедры (GET)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var department = await _context.Departments
            .Include(d => d.Professors)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
            return NotFound();

        return View(department);
    }

    /// <summary>
    /// Удаление кафедры (POST)
    /// </summary>
    [HttpPost("Departments/DeleteConfirmed/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Professors)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return NotFound();

        // Проверка: если есть связанные преподаватели, запретить удаление
        if (department.Professors.Any())
        {
            TempData["Error"] = "Невозможно удалить кафедру, так как с ней связаны преподаватели";
            return RedirectToAction(nameof(Delete), new { id = id });
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }
}
