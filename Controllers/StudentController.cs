using Microsoft.AspNetCore.Mvc;
using mvc.Repositories.Interfaces;

namespace mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        // поле для зберігання посилання на репозиторій, який реалізує інтерфейс IRepository.
        // використання інтерфейсу замість конкретної реалізації дозволяє легко замінити репозиторій
        // в майбутньому (наприклад, на mock для тестів або на іншу реалізацію з іншою логікою/БД),
        // без зміни коду контролера – це ключовий принцип Dependency Inversion (D у SOLID)!
        private readonly IEntityRepository<Student> repo;

        // конструктор з ін'єкцією залежності: ASP.NET Core DI-контейнер автоматично передасть
        // зареєстровану (в program.cs) реалізацію IRepository. це робить контролер незалежним від деталей
        // створення репозиторію і спрощує юніт-тестування (можна буде передати mock-об'єкт)
        public StudentController(IEntityRepository<Student> r)
        {
            repo = r;
        }

        // GET: api/Student
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            var model = await repo.GetEntityListAsync();
            return Ok(model);
        }

        // GET: api/Student/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var list = await repo.GetEntityListAsync();
            if (list == null)
            {
                return NotFound();
            }

            var student = await repo.GetEntityAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }

        // POST: api/Student
        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent([Bind("Id,Name,Surname,Age,GPA")] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // додаємо новий об'єкт через репозиторій.
            // профіт від інтерфейсу: логіка створення (валідація, генерація Id тощо)
            // може бути винесена в окремий клас, а не дублюватися в контролері
            await repo.CreateAsync(student);
            await repo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }

        // PUT: api/Student/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [Bind("Id,Name,Surname,Age,GPA")] Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await StudentExists(id))
            {
                return NotFound();
            }

            // оновлення через репозиторій – вся робота з Firebase
            // захована в реалізації IRepository. це зменшує зв'язність контролера
            // з конкретним сховищем даних і полегшує підтримку/рефакторинг.
            repo.Update(student);
            await repo.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Student/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var list = await repo.GetEntityListAsync();
            if (list == null)
            {
                return Problem("Колекція студентів порожня.");
            }

            var student = await repo.GetEntityAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            // видалення також через інтерфейс репозиторію.
            // в майбутньому можна додати логіку (наприклад, м'яке видалення, логування),
            // змінивши лише реалізацію, а не контролер.
            await repo.DeleteAsync(id);
            await repo.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> StudentExists(int id)
        {
            List<Student> list = await repo.GetEntityListAsync();
            return (list?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}