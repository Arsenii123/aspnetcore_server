using System.ComponentModel.DataAnnotations;

namespace mvc
{
    public class Student
    {
        public int Id { get; set; }

        [Display(Name="Ім'я")]
        public string? Name { get; set; }

        [Display(Name = "Прізвище")]
        public string? Surname { get; set; }

        [Display(Name = "Вік")]
        public int Age { get; set; }

        [Display(Name = "Середній бал")]
        public double GPA { get; set; }
    }
}