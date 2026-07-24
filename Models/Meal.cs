using System.ComponentModel.DataAnnotations;

namespace CaloriesTracker.Models
{
    public class Meal
    {
        public string? Id { get; set; }

        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Meal name is required.")]
        public string MealName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Meal type is required.")]
        public MealType MealType { get; set; }

        [Range(0, 5000, ErrorMessage = "Calories must be between 0 and 5,000.")]
        public int Calories { get; set; }

        [Required(ErrorMessage = "Fullness is required.")]
        public Fullness Fullness { get; set; }
    }
}
