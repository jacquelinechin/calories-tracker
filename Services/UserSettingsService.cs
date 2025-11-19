using CaloriesTracker.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using static Supabase.Postgrest.Constants;

namespace CaloriesTracker.Services
{
    public class UserSettingsService
    {
        private readonly Supabase.Client _supabase;
        private readonly IJSRuntime _js;
        private readonly AuthService _authService;

        public UserSettingsService(Supabase.Client supabase, IJSRuntime js, AuthService authService)
        {
            _supabase = supabase;
            _js = js;
            _authService = authService;
        }

        public async Task<int> GetDailyCalorieGoalAsync()
        {
            var user = await _authService.GetCurrentUserAsync();

            if (user != null)
            {
                var response = await _supabase.From<UserSettings>()
                    .Filter("user_id", Operator.Equals, user.Id)
                    .Get();
                return response.Models.FirstOrDefault()?.DailyCalorieGoal ?? 0;
            }
            else
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "guest_settings");
                if (string.IsNullOrEmpty(json)) return 0;
                return JsonSerializer.Deserialize<UserSettings>(json)?.DailyCalorieGoal ?? 0;
            }
        }

        public async Task<bool> UpsertDailyCalorieGoalAsync(int dailyCalorieGoal)
        {
            var user = await _authService.GetCurrentUserAsync();
            bool success;

            if (user != null)
            {
                var userSettings = new UserSettings
                {
                    UserId = Guid.Parse(user.Id),
                    DailyCalorieGoal = dailyCalorieGoal
                };

                var response = await _supabase.From<UserSettings>().Upsert(userSettings);
                success = true;
            }
            else
            {
                var userSettingsLocal = new UserSettingsLocal
                {
                    DailyCalorieGoal = dailyCalorieGoal
                };

                await _js.InvokeVoidAsync("localStorage.setItem", "guest_settings", JsonSerializer.Serialize(userSettingsLocal));
                success = true;
            }

            return success;
        }

        //public static UserSettingsLocal ToLocal(UserSettings userSettings) => new UserSettingsLocal
        //{
        //    DailyCalorieGoal = userSettings.DailyCalorieGoal
        //};
    }
}
