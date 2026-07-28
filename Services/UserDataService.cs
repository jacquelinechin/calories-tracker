using CaloriesTracker.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace CaloriesTracker.Services
{
    public class UserDataService
    {
        private readonly IJSRuntime _js;
        private readonly FirebaseAuthService _authService;

        public UserDataService(IJSRuntime js, FirebaseAuthService authService)
        {
            _js = js;
            _authService = authService;
        }

        public async Task<int> GetDailyCalorieGoalAsync()
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    var data = await _js.InvokeAsync<JsonElement?>("firebaseInterop.getUserData", _authService.UserId!);
                    if (data.HasValue && data.Value.TryGetProperty("dailyCalorieGoal", out var goalProp)
                        && goalProp.ValueKind == JsonValueKind.Number && goalProp.TryGetInt32(out var goal))
                    {
                        return goal;
                    }
                    return 0;
                }
                else
                {
                    var json = await _js.InvokeAsync<string>("localStorage.getItem", "guest_daily_calorie_goal");
                    if (string.IsNullOrEmpty(json)) return 0;
                    return JsonSerializer.Deserialize<int?>(json) ?? 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting daily calorie goal: {e.Message}");
                return 0;
            }
        }

        public async Task<FirebaseResult> SetDailyCalorieGoalAsync(int goal)
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    return await _js.InvokeAsync<FirebaseResult>("firebaseInterop.setDailyCalorieGoal", _authService.UserId!, goal).AsTask();
                }
                else
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_daily_calorie_goal", goal);
                    return new FirebaseResult { Success = true };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting daily calorie goal: {e.Message}");
                return new FirebaseResult { Success = false, Error = e.Message };
            }
        }

        public async Task<FirebaseResult> AddMealAsync(Meal meal)
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    return await _js.InvokeAsync<FirebaseResult>("firebaseInterop.addMeal",
                        _authService.UserId!, meal.Date, meal.MealName, meal.MealType, meal.Calories, meal.Fullness).AsTask();
                }
                else
                {
                    var localMeals = await GetMealsAsync();
                    meal.Id = Guid.NewGuid().ToString();
                    localMeals.Add(meal);
                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
                    return new FirebaseResult { Success = true };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error adding meal: {e.Message}");
                return new FirebaseResult { Success = false, Error = e.Message };
            }
        }

        public async Task<List<Meal>> GetMealsAsync()
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    return await _js.InvokeAsync<List<Meal>>("firebaseInterop.getMeals", _authService.UserId!).AsTask();
                }
                else
                {
                    var json = await _js.InvokeAsync<string>("localStorage.getItem", "guest_meals");
                    if (string.IsNullOrEmpty(json)) return new List<Meal>();
                    return JsonSerializer.Deserialize<List<Meal>>(json) ?? new List<Meal>();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting meals: {e.Message}");
                return new List<Meal>();
            }
        }

        public async Task<FirebaseResult> UpdateMealAsync(Meal meal)
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    return await _js.InvokeAsync<FirebaseResult>("firebaseInterop.updateMeal",
                        _authService.UserId!, meal.Id, meal.Date, meal.MealName, meal.MealType, meal.Calories, meal.Fullness).AsTask();
                }
                else
                {
                    var localMeals = await GetMealsAsync();
                    var existing = localMeals.FirstOrDefault(m => m.Id == meal.Id);
                    if (existing == null) return new FirebaseResult { Success = false, Error = "Data not found in local storage" };

                    existing.Date = meal.Date;
                    existing.MealName = meal.MealName;
                    existing.MealType = meal.MealType;
                    existing.Calories = meal.Calories;
                    existing.Fullness = meal.Fullness;

                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
                    return new FirebaseResult { Success = true };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error updating meal: {e.Message}");
                return new FirebaseResult { Success = false, Error = e.Message };
            }
        }

        public async Task<FirebaseResult> DeleteMealAsync(string mealId)
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    return await _js.InvokeAsync<FirebaseResult>("firebaseInterop.deleteMeal", _authService.UserId!, mealId).AsTask();
                }
                else
                {
                    var localMeals = await GetMealsAsync();
                    localMeals.RemoveAll(m => m.Id == mealId);
                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
                    return new FirebaseResult { Success = true };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting meal: {e.Message}");
                return new FirebaseResult { Success = false, Error = e.Message };
            }
        }

        public async Task<int> GetFirebaseMealCountAsync()
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    var meals = await _js.InvokeAsync<List<Meal>>("firebaseInterop.getMeals", _authService.UserId!).AsTask();
                    return meals.Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting Firebase meal count: {e.Message}");
                return 0;
            }
        }
        public async Task<int> GetLocalMealCountAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "guest_meals");
                if (string.IsNullOrEmpty(json)) return 0;
                var meals = JsonSerializer.Deserialize<List<Meal>>(json) ?? new List<Meal>();
                return meals.Count;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting local storage meal count: {e.Message}");
                return 0;
            }
        }

        public async Task<FirebaseResult> DeleteAllFirebaseMealAsync()
        {
            try
            {
                if (_authService.IsLoggedIn)
                {
                    return await _js.InvokeAsync<FirebaseResult>("firebaseInterop.deleteAllMeals", _authService.UserId!).AsTask();
                }
                else
                {
                    return new FirebaseResult { Success = false, Error = "You are not logged in." };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting all Firebase meals: {e.Message}");
                return new FirebaseResult { Success = false, Error = e.Message };
            }
        }
        public async Task<FirebaseResult> DeleteAllLocalMealAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "guest_meals");
                return new FirebaseResult { Success = true };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error deleting all local storage meals: {e.Message}");
                return new FirebaseResult { Success = false, Error = e.Message };
            }
        }
    }
}
