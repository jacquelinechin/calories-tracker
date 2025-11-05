using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CaloriesTracker.Pages
{
    public partial class Home
    {
        [Inject]
        public required MealService MealService { get; set; }
        [Inject]
        public required IJSRuntime JS {  get; set; }

        private List<Meal> meals = new();
        private Meal inputMeal = new();
        private int totalCalories;
        private int dailyCaloriesGoal = 1200;
        private bool showMealForm;
        private DateTime selectedDate = DateTime.Today;

        protected override async Task OnInitializedAsync()
        {
            inputMeal = new()
            {
                Date = selectedDate
            };

            meals = await MealService.GetMealsAsync();

            totalCalories = meals
                .Where(x => x.Date.Date == selectedDate.Date)
                .Sum(x => x.Calories);

            MealService.OnDateChanged += date =>
             {
                 selectedDate = date;
                 OnDateChanged();
                 InvokeAsync(StateHasChanged);
             };
        }

        public void Dispose()
        {
            MealService.OnDateChanged -= null;
        }

        private async Task UpsertMeal()
        {
            var mealToUpsert = new Meal
            {
                Id = inputMeal.Id == Guid.Empty ? Guid.NewGuid() : inputMeal.Id,
                Date = inputMeal.Date,
                MealName = inputMeal.MealName,
                MealType = inputMeal.MealType,
                Calories = inputMeal.Calories,
                Fullness = inputMeal.Fullness
            };

            await MealService.UpsertMealAsync(mealToUpsert);
            await OnInitializedAsync();
            MealService.NotifyMealsUpdated();
        }

        private void Clear()
        {
            inputMeal = new()
            {
                Date = selectedDate
            };
        }

        private async Task DeleteMeal(Meal meal)
        {
            bool confirmed = await JS.InvokeAsync<bool>("confirm", $"Delete {meal.MealName}?");
            if (!confirmed)
                return;

            await MealService.DeleteMealAsync(meal.Id);
            await OnInitializedAsync();
            MealService.NotifyMealsUpdated();
        }

        private async Task EditMealAsync(Meal meal)
        {
            showMealForm = true;
            inputMeal = meal;
            await JS.InvokeVoidAsync("scrollToElementWithOffset", "meal-form", 60);
        }

        private void OnDateChanged()
        {
            Clear();
            totalCalories = meals
                .Where(x => x.Date.Date == selectedDate.Date)
                .Sum(x => x.Calories);
        }
    }
}