
using CaloriesTracker.Models;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace CaloriesTracker.Pages
{
    public partial class Settings
    {
        [Inject]
        public required IJSRuntime JS { get; set; }
        [Inject]
        public required AuthService AuthService { get; set; }
        [Inject]
        public required MealService MealService { get; set; }

        private Supabase.Gotrue.User? user;
        private string email = "";
        private string password = "";

        protected override async Task OnInitializedAsync()
        {
            user = await AuthService.GetCurrentUserAsync();
        }

        private async Task HandleLoginAsync()
        {
            try
            {
                user = await AuthService.LoginAsync(email, password);
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<SupabaseError>(ex.Message);
                await JS.InvokeVoidAsync("alert", "Error: " + error?.Message);
            }
        }

        private async Task HandleSignUpAsync()
        {
            try
            {
                user = await AuthService.SignUpAsync(email, password);
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<SupabaseError>(ex.Message);
                await JS.InvokeVoidAsync("alert", "Error: " + error?.Message);
            }
        }

        private async Task HandleLogoutAsync()
        {
            await AuthService.LogoutAsync();
            user = null;
        }

        private async Task DeleteDataAsync()
        {
            bool confirmed = await JS.InvokeAsync<bool>("confirm", "Delete all meal data?");
            if (!confirmed)
                return;

            await MealService.DeleteAllUserMealAsync();
        }
    }
}