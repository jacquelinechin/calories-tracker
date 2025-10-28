using Blazored.LocalStorage;
using CaloriesTracker;
using CaloriesTracker.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Supabase;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();

var supabaseUrl = "https://uqjnsjxojoktutzzxzsa.supabase.co";
var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVxam5zanhvam9rdHV0enp4enNhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTk4MTY2NDgsImV4cCI6MjA3NTM5MjY0OH0.xC5ADfSiVYZtJ7wcEMMbRKRg85Hn8cEpZNzBUTrmg_Y";

var options = new SupabaseOptions
{
    AutoConnectRealtime = false
};

var supabase = new Supabase.Client(supabaseUrl, supabaseKey, options);
await supabase.InitializeAsync();

builder.Services.AddSingleton(supabase);
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MealService>();

var js = builder.Services.BuildServiceProvider().GetRequiredService<IJSRuntime>();

// Try restore saved session
var savedSessionJson = await js.InvokeAsync<string>("localStorage.getItem", "supabase_session");
if (!string.IsNullOrEmpty(savedSessionJson))
{
    var session = JsonSerializer.Deserialize<Supabase.Gotrue.Session>(savedSessionJson);
    if (session != null && session.AccessToken != null && session.RefreshToken != null)
    {
        await supabase.Auth.SetSession(session.AccessToken, session.RefreshToken);
    }
}

await builder.Build().RunAsync();
