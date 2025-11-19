using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;

namespace CaloriesTracker.Models
{
    [Table("user_settings")]
    public class UserSettings : BaseModel
    {

        [PrimaryKey("user_id")]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("daily_calorie_goal")]
        [Range(0, 5000, ErrorMessage = "Daily Calorie Goal must be between 0 and 5,000.")]
        public int? DailyCalorieGoal { get; set; }
    }
}
