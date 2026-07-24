using CaloriesTracker.Models;
using Microsoft.JSInterop;

namespace CaloriesTracker.Services
{
    public class FirebaseAuthService
    {
        private readonly IJSRuntime _js;
        private DotNetObjectReference<FirebaseAuthService>? _objRef;

        public string? UserId { get; private set; }
        public string? Email { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsLoggedIn => UserId != null;

        public event Action? AuthStateChanged;

        public FirebaseAuthService(IJSRuntime js) => _js = js;

        public async Task InitializeAsync()
        {
            _objRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("firebaseInterop.init", _objRef);
        }

        [JSInvokable]
        public void OnAuthStateChanged(string? uid, string? email)
        {
            UserId = uid;
            Email = email;
            IsInitialized = true;
            AuthStateChanged?.Invoke();
        }

        public Task<FirebaseResult> SignInAsync(string email, string password) =>
            _js.InvokeAsync<FirebaseResult>("firebaseInterop.signIn", email, password).AsTask();

        public Task<FirebaseResult> SignUpAsync(string email, string password) =>
            _js.InvokeAsync<FirebaseResult>("firebaseInterop.signUp", email, password).AsTask();

        public Task SignOutAsync() =>
            _js.InvokeVoidAsync("firebaseInterop.signOut").AsTask();
    }
}
