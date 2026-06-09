

## Краткий обзор приложения

### Архитектура

Криент-серверное монолитное приложение.


```text
Браузер (HTML + api.js + app.js)
        ↕ HTTP запросы (JSON)
ASP.NET Core сервер
        ↕ EF Core (C# объекты → SQL)
SQLite база данных (app.db файл)
```

Backend - ASP.NET Core
Frontend - HTML/JS


### API Endpoints


#### /api/departments

| Метод   | Маршрут                   | Что делает                                    |
|---------|---------------------------|-----------------------------------------------|
| GET     | /api/departments          | Все кафедры, сортировка по имени              |
| GET     | /api/departments/{id}     | Одна кафедра + её преподаватели               |
| POST    | /api/departments          | Создать кафедру                               |
| PUT     | /api/departments/{id}     | Обновить название                             |
| DELETE  | /api/departments/{id}     | Удалить (если нет преподавателей)             |


#### /api/professors

| Метод   | Маршрут                   | Что делает                                              |
|---------|---------------------------|---------------------------------------------------------|
| GET     | /api/professors           | Все преподаватели + название кафедры, сортировка по имени |
| GET     | /api/professors/{id}      | Один преподаватель + название кафедры                   |
| POST    | /api/professors           | Создать преподавателя                                   |
| PUT     | /api/professors/{id}      | Обновить данные                                         |
| DELETE  | /api/professors/{id}      | Удалить                                                 |

#### /api/reports

| Метод   | Маршрут                   | Что делает                                                          |
|---------|---------------------------|----------------------------------------------------------------------|
| GET     | /api/reports              | Все три раздела сразу                                                |
| GET     | /api/reports/section1     | Список всех преподавателей с кафедрой                                |
| GET     | /api/reports/section2     | Количество преподавателей по кафедрам                                |
| GET     | /api/reports/section3     | Среднее публикации по кафедрам (по убыванию)                         |



#### Применр получения всех преподавателей

1. ProfessorsController.cs

```cs
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
```

Структура Http ответа

```text
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[{"id":1,"name":"Алексей Васильев","departmentId":1,"departmentName":"Информатика","publications":18},{"id":2,"name":"Анна Лебедева","departmentId":2,"departmentName":"Математика","publications":30}]
```

2.1 api.js

```js
const response = await fetch(`${API_BASE}/professors`);
return await response.json();
```


2.2 app.js

```js
const professors = await api.getProfessors();
this.renderProfessorsList(professors);
```



