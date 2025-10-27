using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components;

namespace CaloriesTracker.Pages
{
    public partial class Home
    {
        //[Inject]
        //public required Blazored.LocalStorage.ILocalStorageService localStorage { get; set; }
        //[Inject]
        //public required Supabase.Client SupabaseClient { get; set; }
        //[Inject]
        //public required AuthService AuthService { get; set; }
        [Inject]
        public required MealService MealService { get; set; }

        //private Supabase.Gotrue.User? user;
        private List<Meal> meals = new();
        private Meal inputMeal = new();
        private int totalCalories;
        private int dailyCaloriesGoal;

        protected override async Task OnInitializedAsync()
        {
            //data = await localStorage.GetItemAsync<List<MealItem>>(DateTime.Today.ToString("dd-MM-yyyy")) ?? new List<MealItem>();
            //dailyCaloriesGoal = await localStorage.GetItemAsync<int?>("dailyCaloriesGoal") ?? 0;

            //user = await AuthService.GetCurrentUserAsync();

            //var response = await SupabaseClient.From<Meal>().Get();
            //meals = response.Models;

            meals = await MealService.GetMealsAsync();

            foreach (var item in meals)
            {
                totalCalories += item.Calories;
            }
        }

        private async Task UpsertMeal(Meal? meal = null)
        {
            //data.Add(newMeal);
            //await localStorage.SetItemAsync(DateTime.Today.ToString("dd-MM-yyyy"), data);

            //newItem = new();
            //totalCalories += newItem.Calories;

            //var newMeal = new Meal
            //{
            //    UserId = Guid.Parse(user?.Id),
            //    Date = DateTime.UtcNow.Date,
            //    MealName = "Example Meal",
            //    MealType = MealType.Breakfast,
            //    Calories = 400,
            //    Fullness = Fullness.Full
            //};

            //await SupabaseClient.From<Meal>().Insert(newMeal);
            //meals.Add(newMeal);

            var mealToUpsert = new Meal();

            if (meal == null)
            {
                mealToUpsert = new Meal
                {
                    Id = Guid.NewGuid(),
                    Date = inputMeal.Date,
                    MealName = inputMeal.MealName,
                    MealType = inputMeal.MealType,
                    Calories = inputMeal.Calories,
                    Fullness = inputMeal.Fullness
                };
            }
            else
            {
                mealToUpsert = meal;
            }

            await MealService.UpsertMealAsync(mealToUpsert);
            meals = await MealService.GetMealsAsync();
            inputMeal = new();
        }

        private async Task DeleteMeal(Guid mealId)
        {
            await MealService.DeleteMealAsync(mealId);

            meals = await MealService.GetMealsAsync();
            totalCalories = 0;
            foreach (var item in meals)
            {
                totalCalories += item.Calories;
            }
        }

        private void SetFullness(Fullness level)
        {
            inputMeal.Fullness = (int)level;
        }
    }
}