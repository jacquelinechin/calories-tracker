using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CaloriesTracker.Pages
{
    public partial class Settings
    {
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public FirebaseAuthService AuthService { get; set; } = default!;
        [Inject] public UserDataService UserDataService { get; set; } = default!;

        private string email = "";
        private string password = "";
        private int dailyCalorieGoal;
        private int TotalLocalMeals { get; set; } = 0;
        private int TotalFirebaseMeals { get; set; } = 0;
        private EditForm? form;

        protected override async Task OnInitializedAsync()
        {
            AuthService.AuthStateChanged += OnAuthChanged;
            await LoadGoal();
            await LoadMeals();
        }

        private async void OnAuthChanged()
        {
            await LoadGoal();
            await LoadMeals();
        }

        private async Task LoadGoal()
        {
            dailyCalorieGoal = await UserDataService.GetDailyCalorieGoalAsync();
            StateHasChanged();
        }

        private async Task SaveGoal()
        {
            var result = await UserDataService.SetDailyCalorieGoalAsync(dailyCalorieGoal);
            var statusMessage = result.Success ? "Saved!" : "Error: " + result.Error;
            await JS.InvokeVoidAsync("alert", statusMessage);
        }

        private async Task LoadMeals()
        {
            TotalLocalMeals = await UserDataService.GetLocalMealCountAsync();
            TotalFirebaseMeals = await UserDataService.GetFirebaseMealCountAsync();
            StateHasChanged();
        }

        private async Task HandleLoginAsync()
        {
            var result = await AuthService.SignInAsync(email, password);
            if (!result.Success)
            {
                var errorMessage = result.Error switch
                {
                    "auth/invalid-credential" => "Incorrect email or password.",
                    "auth/too-many-requests" => "Too many attempts. Try again later.",
                    _ => "Something went wrong. Please try again."
                };

                await JS.InvokeVoidAsync("alert", "Error: " + errorMessage);
            }
        }

        private async Task HandleSignUpAsync()
        {
            var result = await AuthService.SignUpAsync(email, password);
            if (!result.Success)
            {
                var errorMessage = result.Error switch
                {
                    "auth/email-already-in-use" => "That email is already registered.",
                    "auth/weak-password" => "Password must be at least 6 characters.",
                    "auth/invalid-email" => "Please enter a valid email.",
                    _ => "Something went wrong. Please try again."
                };

                await JS.InvokeVoidAsync("alert", "Error: " + errorMessage);
            }
        }

        private async Task HandleLogoutAsync()
        {
            await AuthService.SignOutAsync();
        }

        private async Task DeleteLocalDataAsync()
        {
            bool confirmed = await JS.InvokeAsync<bool>("confirm", "Delete all local storage meal data?");
            if (!confirmed)
                return;

            var result = await UserDataService.DeleteAllLocalMealAsync();
            if (result.Success)
                await LoadMeals();
            else
                await JS.InvokeVoidAsync("alert", "Error: " + result.Error);
        }

        private async Task DeleteFirebaseDataAsync()
        {
            bool confirmed = await JS.InvokeAsync<bool>("confirm", "Delete all Firebase meal data?");
            if (!confirmed)
                return;

            var result = await UserDataService.DeleteAllFirebaseMealAsync();
            if (result.Success)
                await LoadMeals();
            else
                await JS.InvokeVoidAsync("alert", "Error: " + result.Error);
        }

        public void Dispose()
        {
            AuthService.AuthStateChanged -= OnAuthChanged;
        }
    }
}