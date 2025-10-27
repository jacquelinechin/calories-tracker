using Microsoft.JSInterop;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;
using System.Text.Json;

namespace CaloriesTracker.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabase;
        private readonly IJSRuntime _js;

        public AuthService(Supabase.Client supabase, IJSRuntime js)
        {
            _supabase = supabase;
            _js = js;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var session = _supabase.Auth.CurrentSession;
            if (session == null)
            {
                // Try restore from localStorage
                var saved = await _js.InvokeAsync<string>("localStorage.getItem", "supabase_session");
                if (!string.IsNullOrEmpty(saved))
                {
                    var restored = JsonSerializer.Deserialize<Session>(saved);
                    if (restored != null)
                    {
                        await _supabase.Auth.SetSession(restored.AccessToken, restored.RefreshToken);
                        return await _supabase.Auth.GetUser(restored.AccessToken);
                    }
                }
                return null;
            }

            return await _supabase.Auth.GetUser(session.AccessToken);
        }

        public bool IsLoggedIn => _supabase.Auth.CurrentSession != null;

        public async Task LogoutAsync()
        {
            await _supabase.Auth.SignOut();
            await _js.InvokeVoidAsync("localStorage.removeItem", "supabase_session");
        }
    }
}
