using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components;

namespace CaloriesTracker.Pages
{
    public partial class Home
    {
        [Inject]
        public required MealService MealService { get; set; }

        private List<Meal> meals = new();
        private Meal inputMeal = new();
        private int totalCalories;
        private int dailyCaloriesGoal;

        protected override async Task OnInitializedAsync()
        {
            inputMeal = new();
            totalCalories =  0;

            meals = await MealService.GetMealsAsync();

            foreach (var item in meals)
            {
                totalCalories += item.Calories;
            }
        }

        private async Task UpsertMeal(Meal? meal = null)
        {
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
            await OnInitializedAsync();
        }

        private async Task DeleteMeal(Guid mealId)
        {
            await MealService.DeleteMealAsync(mealId);
            await OnInitializedAsync();
        }

        private void SetFullness(Fullness level)
        {
            inputMeal.Fullness = (int)level;
        }
    }
}