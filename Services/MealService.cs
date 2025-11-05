using CaloriesTracker.Models;
using Microsoft.JSInterop;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;
using Supabase.Postgrest;
using System.Text.Json;
using static Supabase.Postgrest.Constants;

namespace CaloriesTracker.Services
{
    public class MealService
    {
        private readonly Supabase.Client _supabase;
        private readonly IJSRuntime _js;
        private readonly AuthService _authService;
        public event Action<DateTime>? OnDateChanged;
        public event Action? OnMealsUpdated;

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnDateChanged?.Invoke(value);
                }
            }
        }

        public MealService(Supabase.Client supabase, IJSRuntime js, AuthService authService)
        {
            _supabase = supabase;
            _js = js;
            _authService = authService;
        }

        public void NotifyMealsUpdated()
        {
            OnMealsUpdated?.Invoke();
        }

        public async Task<List<Meal>> GetMealsAsync()
        {
            var user = await _authService.GetCurrentUserAsync();

            if (user != null)
            {
                var response = await _supabase.From<Meal>()
                    .Filter("user_id", Operator.Equals, user.Id)
                    .Get();
                return response.Models;
            }
            else
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "guest_meals");
                if (string.IsNullOrEmpty(json)) return new List<Meal>();
                return JsonSerializer.Deserialize<List<Meal>>(json) ?? new List<Meal>();
            }
        }

        public async Task UpsertMealAsync(Meal meal)
        {
            var user = await _authService.GetCurrentUserAsync();

            meal.Date = DateTime.SpecifyKind(meal.Date, DateTimeKind.Utc);

            if (user != null)
            {
                meal.UserId = Guid.Parse(user.Id);

                var response = await _supabase.From<Meal>().Upsert(meal);

                //if (response.Models.Any())
                //{
                //    // Update the local meal with the DB-generated ID (for new inserts)
                //    var savedMeal = response.Models.First();
                //    meal.Id = savedMeal.Id;
                //    meal.UserId = savedMeal.UserId;
                //}
            }
            else
            {
                var localMeals = (await GetMealsAsync()).Select(ToLocal).ToList();

                var existing = localMeals.FirstOrDefault(m => m.Id == meal.Id);
                if (existing != null)
                {
                    // Update existing
                    existing.Date = meal.Date;
                    existing.MealName = meal.MealName;
                    existing.MealType = meal.MealType;
                    existing.Calories = meal.Calories;
                    existing.Fullness = meal.Fullness;
                }
                else
                {
                    localMeals.Add(ToLocal(meal));
                }
                await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
            }
        }

        public async Task DeleteMealAsync(Guid mealId)
        {
            var user = await _authService.GetCurrentUserAsync();

            if (user != null)
            {
                await _supabase.From<Meal>()
                    .Filter("id", Operator.Equals, mealId.ToString())
                    .Filter("user_id", Operator.Equals, user.Id)
                    .Delete();
            }
            else
            {
                var localMeals = (await GetMealsAsync()).Select(ToLocal).ToList();
                localMeals.RemoveAll(m => m.Id == mealId);
                await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
            }
        }

        public static MealLocal ToLocal(Meal meal) => new MealLocal
        {
            Id = meal.Id,
            Date = meal.Date,
            MealName = meal.MealName,
            MealType = meal.MealType,
            Calories = meal.Calories,
            Fullness = meal.Fullness
        };

        public static Meal ToMeal(MealLocal local) => new Meal
        {
            Id = local.Id,
            Date = local.Date,
            MealName = local.MealName,
            MealType = local.MealType,
            Calories = local.Calories,
            Fullness = local.Fullness
        };
    }
}
