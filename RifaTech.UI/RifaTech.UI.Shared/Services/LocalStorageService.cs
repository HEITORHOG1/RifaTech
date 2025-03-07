using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return await _localStorage.GetItemAsync<T>(key);
        }

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
