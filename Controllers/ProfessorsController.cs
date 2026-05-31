using homework3.Data;
using homework3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace homework3.Controllers;

/// <summary>
/// REST API контроллер для управления преподавателями
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProfessorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProfessorsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список всех преподавателей
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetProfessors()
    {
        var professors = await _context.Professors
            .Include(p => p.Department)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.DepartmentId,
                DepartmentName = p.Department!.Name,
                p.Publications
            })
            .ToListAsync();
        return Ok(professors);
    }

    /// <summary>
    /// Получить преподавателя по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetProfessor(int id)
    {
        var professor = await _context.Professors
            .Include(p => p.Department)
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.DepartmentId,
                DepartmentName = p.Department!.Name,
                p.Publications
            })
            .FirstOrDefaultAsync();

        if (professor == null)
            return NotFound(new { message = "Преподаватель не найден" });

        return Ok(professor);
    }

    /// <summary>
    /// Создать нового преподавателя
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Professor>> CreateProfessor([FromBody] Professor professor)
    {
        if (string.IsNullOrWhiteSpace(professor.Name))
            return BadRequest(new { message = "Имя преподавателя не может быть пустым" });

        if (professor.Publications < 0)
            return BadRequest(new { message = "Количество публикаций не может быть отрицательным" });

        if (professor.DepartmentId <= 0)
            return BadRequest(new { message = "Выберите кафедру" });

        var departmentExists = await _context.Departments.AnyAsync(d => d.Id == professor.DepartmentId);
        if (!departmentExists)
            return BadRequest(new { message = "Выбранная кафедра не существует" });

        _context.Professors.Add(professor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProfessor), new { id = professor.Id }, professor);
    }

    /// <summary>
    /// Обновить преподавателя
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfessor(int id, [FromBody] Professor professor)
    {
        if (id != professor.Id)
            return BadRequest(new { message = "ID не совпадает" });

        if (string.IsNullOrWhiteSpace(professor.Name))
            return BadRequest(new { message = "Имя преподавателя не может быть пустым" });

        if (professor.Publications < 0)
            return BadRequest(new { message = "Количество публикаций не может быть отрицательным" });

        if (professor.DepartmentId <= 0)
            return BadRequest(new { message = "Выберите кафедру" });

        var existing = await _context.Professors.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = "Преподаватель не найден" });

        var departmentExists = await _context.Departments.AnyAsync(d => d.Id == professor.DepartmentId);
        if (!departmentExists)
            return BadRequest(new { message = "Выбранная кафедра не существует" });

        existing.Name = professor.Name;
        existing.DepartmentId = professor.DepartmentId;
        existing.Publications = professor.Publications;

        _context.Professors.Update(existing);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Преподаватель успешно обновлён" });
    }

    /// <summary>
    /// Удалить преподавателя
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfessor(int id)
    {
        var professor = await _context.Professors.FindAsync(id);
        if (professor == null)
            return NotFound(new { message = "Преподаватель не найден" });

        _context.Professors.Remove(professor);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Преподаватель успешно удалён" });
    }
}
