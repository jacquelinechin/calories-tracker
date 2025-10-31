using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;

namespace CaloriesTracker.Models
{
    [Table("meals")]
    public class Meal : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("date")]
        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Column("meal_name")]
        [Required(ErrorMessage = "Meal name is required.")]
        [StringLength(100, ErrorMessage = "Meal name must be less than 100 characters.")]
        public string MealName { get; set; } = string.Empty;

        [Column("meal_type")]
        [Required(ErrorMessage = "Meal type is required.")]
        public MealType MealType { get; set; }

        [Column("calories")]
        [Range(0, 5000, ErrorMessage = "Calories must be between 0 and 5,000.")]
        public int Calories { get; set; }

        [Column("fullness")]
        [Required(ErrorMessage = "Fullness is required.")]
        public Fullness Fullness { get; set; }
    }
}
