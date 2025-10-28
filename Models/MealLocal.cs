namespace CaloriesTracker.Models
{
    public class MealLocal
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public string MealName { get; set; } = string.Empty;

        public MealType MealType { get; set; }

        public int Calories { get; set; }

        public Fullness Fullness { get; set; }
    }
}
