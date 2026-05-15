namespace homework3.Models;

/// <summary>
/// Преподаватель (основная таблица, сторона «много»)
/// </summary>
public class Professor
{
    /// <summary>
    /// Идентификатор преподавателя (первичный ключ)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор кафедры (внешний ключ)
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Навигационное свойство: кафедра преподавателя
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Имя преподавателя
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Количество научных публикаций
    /// </summary>
    public int Publications { get; set; }
}
