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
            if (_authService.IsLoggedIn)
            {
                var data = await _js.InvokeAsync<JsonElement?>("firebaseInterop.getUserData", _authService.UserId!);

                if (data.HasValue && data.Value.TryGetProperty("dailyCalorieGoal", out var goalProp))
                {
                    return goalProp.GetInt32();
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

        public async Task<FirebaseResult> SetDailyCalorieGoalAsync(int goal)
        {
            if (_authService.IsLoggedIn)
            {
                return await _js.InvokeAsync<FirebaseResult>("firebaseInterop.setDailyCalorieGoal", _authService.UserId!, goal).AsTask();
            }
            else
            {
                try
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_daily_calorie_goal", goal);
                    return new FirebaseResult { Success = true };
                }
                catch (Exception e)
                {
                    return new FirebaseResult { Success = false, Error = e.Message };
                }
            }   
        }

        public async Task<FirebaseResult> AddMealAsync(Meal meal)
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
                try
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
                    return new FirebaseResult { Success = true };
                }
                catch (Exception e)
                {
                    return new FirebaseResult { Success = false, Error = e.Message };
                }
            }
        }

        public async Task<List<Meal>> GetMealsAsync()
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

        public async Task<FirebaseResult> UpdateMealAsync(Meal meal)
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

                try
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
                    return new FirebaseResult { Success = true };
                }
                catch (Exception e)
                {
                    return new FirebaseResult { Success = false, Error = e.Message };
                }
            }
        }

        public async Task<FirebaseResult> DeleteMealAsync(string mealId)
        {
            if (_authService.IsLoggedIn)
            {
                return await _js.InvokeAsync<FirebaseResult>("firebaseInterop.deleteMeal", _authService.UserId!, mealId).AsTask();
            }
            else
            {
                var localMeals = await GetMealsAsync();
                localMeals.RemoveAll(m => m.Id == mealId);
                try
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "guest_meals", JsonSerializer.Serialize(localMeals));
                    return new FirebaseResult { Success = true };
                }
                catch (Exception e)
                {
                    return new FirebaseResult { Success = false, Error = e.Message };
                }
            }
        }

        public async Task<int> GetFirebaseMealCountAsync()
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
        public async Task<int> GetLocalMealCountAsync()
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", "guest_meals");
            if (string.IsNullOrEmpty(json)) return 0;
            var meals = JsonSerializer.Deserialize<List<Meal>>(json) ?? new List<Meal>();
            return meals.Count;
        }

        public async Task<FirebaseResult> DeleteAllFirebaseMealAsync()
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
        public async Task<FirebaseResult> DeleteAllLocalMealAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "guest_meals");
                return new FirebaseResult { Success = true };
            }
            catch (Exception e)
            {
                return new FirebaseResult { Success = false, Error = e.Message };
            }
        }
    }
}
