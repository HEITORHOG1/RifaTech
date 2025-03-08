namespace RifaTech.UI.Shared.Services
{
    // RifaTech.UI.Shared/Services/LocalStorageService.cs

    public class LocalStorageService : ILocalStorageService
    {
        private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;

        public LocalStorageService(Blazored.LocalStorage.ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            try
            {
                return await _localStorage.GetItemAsync<T>(key);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                return default;
            }
        }

        // Do the same for all other methods that call JavaScript functions

        public async Task SetItemAsync<T>(string key, T value)
        {
            await _localStorage.SetItemAsync(key, value);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _localStorage.RemoveItemAsync(key);
        }

        public async Task ClearAsync()
        {
            await _localStorage.ClearAsync();
        }
    }
}
