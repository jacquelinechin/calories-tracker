using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CaloriesTracker.Pages
{
    public partial class Home
    {
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public FirebaseAuthService AuthService { get; set; } = default!;
        [Inject] public UserDataService UserDataService { get; set; } = default!;

        private DateTime currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        private List<DateTime> daysInMonth = new();

        private List<Meal> meals = new();
        private Meal inputMeal = new();
        private int totalCalories;
        private int calorieGoal;
        private bool showMealForm;
        private DateOnly selectedDate = DateOnly.FromDateTime(DateTime.Today);

        protected override async Task OnInitializedAsync()
        {
            AuthService.AuthStateChanged += OnAuthChanged;
            await LoadData();
        }

        private async void OnAuthChanged()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            meals = await UserDataService.GetMealsAsync();
            calorieGoal = await UserDataService.GetCalorieGoalAsync();

            GenerateCalendar();

            totalCalories = meals
                .Where(x => x.Date == selectedDate)
                .Sum(x => x.Calories);

            StateHasChanged();
        }

        private async Task UpsertMeal()
        {
            var mealToUpsert = new Meal
            {
                Id = inputMeal.Id,
                Date = inputMeal.Date,
                MealName = inputMeal.MealName,
                Calories = inputMeal.Calories,
                ProteinGrams = inputMeal.ProteinGrams,
                CarbsGrams = inputMeal.CarbsGrams,
                FatGrams = inputMeal.FatGrams,
                MealType = inputMeal.MealType,
                Fullness = inputMeal.Fullness
            };

            var result = mealToUpsert.Id == null ? await UserDataService.AddMealAsync(mealToUpsert)
                : await UserDataService.UpdateMealAsync(mealToUpsert);
            
            if (result.Success)
            {
                await LoadData();
                Clear();
                await JS.InvokeVoidAsync("alert", "Saved!");
            }
            else
                await JS.InvokeVoidAsync("alert", "Error: " + result.Error);
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

            await UserDataService.DeleteMealAsync(meal.Id!);
            await LoadData();
            Clear();
        }

        private async Task EditMealAsync(Meal meal)
        {
            showMealForm = true;
            inputMeal = new Meal
            {
                Id = meal.Id,
                Date = meal.Date,
                MealName = meal.MealName,
                Calories = meal.Calories,
                ProteinGrams = meal.ProteinGrams,
                CarbsGrams = meal.CarbsGrams,
                FatGrams = meal.FatGrams,
                MealType = meal.MealType,
                Fullness = meal.Fullness
            };
            await JS.InvokeVoidAsync("scrollToElementWithOffset", "meal-form", 60);
        }

        private void OnDateChanged(DateOnly date)
        {
            selectedDate = date;
            Clear();
            totalCalories = meals
                .Where(x => x.Date == selectedDate)
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
            calorieGoal == 0 ? "" :
            total switch
            {
                <= 0 => "",
                _ when total >= calorieGoal => "bg-green",
                _ => "bg-yellow"
            };

        public void Dispose()
        {
            AuthService.AuthStateChanged -= OnAuthChanged;
        }
    }
}