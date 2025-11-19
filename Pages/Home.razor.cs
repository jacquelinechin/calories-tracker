using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CaloriesTracker.Pages
{
    public partial class Home
    {
        [Inject]
        public required MealService MealService { get; set; }
        [Inject]
        public required UserSettingsService UserSettingsService { get; set; }
        [Inject]
        public required IJSRuntime JS {  get; set; }


        private DateTime currentMonth = DateTime.Today;
        private List<DateTime> daysInMonth = new();

        private List<Meal> meals = new();
        private Meal inputMeal = new();
        private int totalCalories;
        private int dailyCalorieGoal;
        private bool showMealForm;
        private DateTime selectedDate = DateTime.Today;

        protected override async Task OnInitializedAsync()
        {
            inputMeal = new()
            {
                Date = selectedDate
            };

            meals = await MealService.GetMealsAsync();
            dailyCalorieGoal = await UserSettingsService.GetDailyCalorieGoalAsync();

            GenerateCalendar();

            totalCalories = meals
                .Where(x => x.Date.Date == selectedDate.Date)
                .Sum(x => x.Calories);
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
        }

        private async Task EditMealAsync(Meal meal)
        {
            showMealForm = true;
            inputMeal = new Meal
            {
                Id = meal.Id,
                Date = meal.Date,
                MealName = meal.MealName,
                MealType = meal.MealType,
                Calories = meal.Calories,
                Fullness = meal.Fullness
            };
            await JS.InvokeVoidAsync("scrollToElementWithOffset", "meal-form", 60);
        }

        private void OnDateChanged(DateTime date)
        {
            selectedDate = date;
            Clear();
            totalCalories = meals
                .Where(x => x.Date.Date == selectedDate.Date)
                .Sum(x => x.Calories);
        }

        private void GenerateCalendar()
        {
            var start = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            daysInMonth = Enumerable.Range(0, (end - start).Days + 1)
                .Select(offset => start.AddDays(offset))
                .ToList();
        }

        private void NextMonth()
        {
            currentMonth = currentMonth.AddMonths(1);
            GenerateCalendar();
        }

        private void PrevMonth()
        {
            currentMonth = currentMonth.AddMonths(-1);
            GenerateCalendar();
        }

        private string GetColor(int total) =>
        total switch
        {
            <= 0 => "",
            _ when total >= dailyCalorieGoal => "bg-green",
            _ => "bg-yellow"
        };
    }
}