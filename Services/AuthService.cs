using Microsoft.JSInterop;
using Supabase.Gotrue;
using System.Text.Json;

namespace CaloriesTracker.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabase;
        private readonly IJSRuntime _js;
        public event Func<Task>? OnAuthStateChanged;

        public AuthService(Supabase.Client supabase, IJSRuntime js)
        {
            _supabase = supabase;
            _js = js;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var session = _supabase.Auth.CurrentSession;
                if (session == null || session.AccessToken == null)
                {
                    // Try restore from localStorage
                    var saved = await _js.InvokeAsync<string>("localStorage.getItem", "supabase_session");
                    if (!string.IsNullOrEmpty(saved))
                    {
                        var restored = JsonSerializer.Deserialize<Session>(saved);
                        if (restored != null && !string.IsNullOrWhiteSpace(restored.AccessToken) && !string.IsNullOrWhiteSpace(restored.RefreshToken))
                        {
                            await _supabase.Auth.SetSession(restored.AccessToken, restored.RefreshToken);
                            return await _supabase.Auth.GetUser(restored.AccessToken);
                        }
                        else
                        {
                            await _js.InvokeVoidAsync("localStorage.removeItem", "supabase_session");
                            return null;
                        }
                    }
                    return null;
                }

                return await _supabase.Auth.GetUser(session.AccessToken);
            }
            catch (Exception)
            {
                //await _js.InvokeVoidAsync("alert", "Error in GetCurrentUserAsync: " + ex.Message);
                await _js.InvokeVoidAsync("localStorage.removeItem", "supabase_session");
                return null;
            }
        }

        public bool IsLoggedIn => _supabase.Auth.CurrentSession != null;

        public async Task<User?> LoginAsync(string email, string password)
        {
            var result = await _supabase.Auth.SignIn(email, password);
            if (result != null && result.AccessToken != null)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "supabase_session", JsonSerializer.Serialize(result));
                await NotifyAuthStateChanged();
                return result.User;
            }
            return null;
        }

        public async Task<User?> SignUpAsync(string email, string password)
        {
            var result = await _supabase.Auth.SignUp(email, password);
            if (result != null && result.AccessToken != null)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "supabase_session", JsonSerializer.Serialize(result));
                await NotifyAuthStateChanged();
                return result.User;
            }
            else
            {
                await _js.InvokeVoidAsync("alert", "Error: User already registered");
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            await _supabase.Auth.SignOut();
            await _js.InvokeVoidAsync("localStorage.removeItem", "supabase_session");
            await NotifyAuthStateChanged();
        }

        private async Task NotifyAuthStateChanged()
        {
            if (OnAuthStateChanged != null)
                await OnAuthStateChanged.Invoke();
        }
    }
}
