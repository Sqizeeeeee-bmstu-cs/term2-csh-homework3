using homework3.Data;
using homework3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework3.Controllers;

/// <summary>
/// REST API контроллер для управления кафедрами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех кафедр
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
    {
        var departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();
        return Ok(departments);
    }

    /// <summary>
    /// Получить кафедру по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Department>> GetDepartment(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Professors)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return NotFound(new { message = "Кафедра не найдена" });

        return Ok(department);
    }

    /// <summary>
    /// Создать новую кафедру
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Department>> CreateDepartment([FromBody] Department department)
    {
        if (string.IsNullOrWhiteSpace(department.Name))
            return BadRequest(new { message = "Название кафедры не может быть пустым" });

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
    }

    /// <summary>
    /// Обновить кафедру
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] Department department)
    {
        if (id != department.Id)
            return BadRequest(new { message = "ID не совпадает" });

        if (string.IsNullOrWhiteSpace(department.Name))
            return BadRequest(new { message = "Название кафедры не может быть пустым" });

        var existing = await _context.Departments.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = "Кафедра не найдена" });

        existing.Name = department.Name;
        _context.Departments.Update(existing);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Кафедра успешно обновлена" });
    }

    /// <summary>
    /// Удалить кафедру
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Professors)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return NotFound(new { message = "Кафедра не найдена" });

        if (department.Professors.Any())
            return BadRequest(new { message = "Невозможно удалить кафедру, так как с ней связаны преподаватели", professorCount = department.Professors.Count });

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Кафедра успешно удалена" });
    }
}
