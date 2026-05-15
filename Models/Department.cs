namespace homework3.Models;

/// <summary>
/// Кафедра (справочная таблица, сторона «один»)
/// </summary>
public class Department
{
    /// <summary>
    /// Идентификатор кафедры (первичный ключ)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название кафедры
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Навигационное свойство: преподаватели этой кафедры
    /// </summary>
    public ICollection<Professor> Professors { get; set; } = new List<Professor>();
}
