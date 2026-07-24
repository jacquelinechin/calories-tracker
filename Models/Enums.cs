using System.Text.Json.Serialization;

namespace CaloriesTracker.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Fullness
    {
        Hungry,
        Peckish,
        Satisfied,
        Full,
        VeryFull
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MealType
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack
    }
}
