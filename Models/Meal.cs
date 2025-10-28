using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

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
        public DateTime Date { get; set; } = DateTime.Today;

        [Column("meal_name")]
        public string MealName { get; set; } = string.Empty;

        [Column("meal_type")]
        public MealType MealType { get; set; }

        [Column("calories")]
        public int Calories { get; set; }

        [Column("fullness")]
        public Fullness Fullness { get; set; }
    }
}
