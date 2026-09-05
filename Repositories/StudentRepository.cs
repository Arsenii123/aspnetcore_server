using Firebase.Database; // dotnet add package FirebaseDatabase.net
using Firebase.Database.Query;
using mvc.Repositories.Interfaces;

namespace mvc.Repository
{
    public class StudentRepository : IEntityRepository<Student>
    {
        private readonly FirebaseClient _firebaseClient;
        private const string CollectionName = "students";
        private const string MetaNode = "students_meta";

        private readonly Lazy<Task> _seedTask;

        public StudentRepository(IConfiguration config)
        {
            // Перевіряємо всі можливі ключі конфігурації
            string databaseUrl = config["FIREBASE_DATABASE_URL"]
                ?? config["Firebase__DatabaseUrl"]
                ?? config["Firebase:DatabaseUrl"]
                ?? throw new InvalidOperationException("Firebase Database URL не налаштовано в змінних оточення чи appsettings.json.");

            _firebaseClient = new FirebaseClient(databaseUrl);

            _seedTask = new Lazy<Task>(EnsureSeededAsync);
        }

        private async Task EnsureSeededAsync()
        {
            bool? alreadySeeded = await _firebaseClient
                .Child(MetaNode)
                .Child("seeded")
                .OnceSingleAsync<bool?>();

            if (alreadySeeded == true)
                return;

            var existing = await _firebaseClient
                .Child(CollectionName)
                .OnceSingleAsync<Dictionary<string, Student>>();

            if (existing == null || existing.Count == 0)
            {
                foreach (var student in GetSeedData())
                {
                    await _firebaseClient
                        .Child(CollectionName)
                        .Child(student.Id.ToString())
                        .PutAsync(student);
                }
            }

            await _firebaseClient
                .Child(MetaNode)
                .Child("seeded")
                .PutAsync(true);
        }

        private Task EnsureSeedWithRetryAsync() => _seedTask.Value;

        private static List<Student> GetSeedData()
        {
            return new List<Student>
            {
                new Student { Id = 1, Name = "Іван",     Surname = "Петренко",  Age = 20, GPA = 4.8 },
                new Student { Id = 2, Name = "Марія",    Surname = "Коваленко", Age = 21, GPA = 4.5 },
                new Student { Id = 3, Name = "Олексій",  Surname = "Шевченко",  Age = 19, GPA = 3.9 },
                new Student { Id = 4, Name = "Оксана",   Surname = "Бондаренко",Age = 22, GPA = 4.2 },
                new Student { Id = 5, Name = "Андрій",   Surname = "Ткаченко",  Age = 20, GPA = 3.6 },
            };
        }

        public async Task<List<Student>> GetEntityListAsync()
        {
            await EnsureSeedWithRetryAsync();

            try
            {
                var studentsDict = await _firebaseClient
                    .Child(CollectionName)
                    .OnceSingleAsync<Dictionary<string, Student>>();

                if (studentsDict == null) return new List<Student>();

                return studentsDict
                    .Where(pair => pair.Value != null)
                    .Select(pair =>
                    {
                        var student = pair.Value;
                        if (student.Id == 0 && int.TryParse(pair.Key, out int id))
                        {
                            student.Id = id;
                        }
                        return student;
                    }).ToList();
            }
            catch
            {
                var studentsList = await _firebaseClient
                    .Child(CollectionName)
                    .OnceSingleAsync<List<Student>>();

                if (studentsList == null) return new List<Student>();

                return studentsList
                    .Where(s => s != null)
                    .ToList();
            }
        }

        public async Task<Student> GetEntityAsync(int id)
        {
            await EnsureSeedWithRetryAsync();

            var student = await _firebaseClient
                .Child(CollectionName)
                .Child(id.ToString())
                .OnceSingleAsync<Student>();

            if (student == null)
                throw new InvalidOperationException($"Студента з айді {id} не знайдено.");

            student.Id = id;
            return student;
        }

        public async Task CreateAsync(Student entity)
        {
            await EnsureSeedWithRetryAsync();

            if (entity.Id == 0)
            {
                var list = await GetEntityListAsync();
                entity.Id = list.Any() ? list.Max(s => s.Id) + 1 : 1;
            }

            await _firebaseClient
                .Child(CollectionName)
                .Child(entity.Id.ToString())
                .PutAsync(entity);
        }

        public void Update(Student entity)
        {
            EnsureSeedWithRetryAsync().GetAwaiter().GetResult();

            _firebaseClient
                .Child(CollectionName)
                .Child(entity.Id.ToString())
                .PutAsync(entity)
                .GetAwaiter()
                .GetResult();
        }

        public async Task DeleteAsync(int id)
        {
            await EnsureSeedWithRetryAsync();

            await _firebaseClient
                .Child(CollectionName)
                .Child(id.ToString())
                .DeleteAsync();
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
