using Microsoft.JSInterop;

namespace RifaTech.UI.Shared.Services
{
    // RifaTech.UI.Shared/Services/LocalStorageService.cs

    public class BrowserStorageService : IStorageService
    {
        private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private bool _isInitialized = false;

        public BrowserStorageService(Blazored.LocalStorage.ILocalStorageService localStorage, IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                try
                {
                    // Verifica se o JS Interop já está disponível
                    await _jsRuntime.InvokeAsync<bool>("window.isJsInteropReady", null);
                    _isInitialized = true;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
                {
                    // Se chegamos aqui, o JS interop não está pronto (provavelmente pré-renderização)
                    // Não fazemos nada e permitimos que o método retorne o valor padrão
                }
            }
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            try
            {
                await EnsureInitializedAsync();
                if (!_isInitialized)
                    return default;

                return await _localStorage.GetItemAsync<T>(key);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                // Silenciosamente falha durante pré-renderização
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter item do localStorage: {ex.Message}");
                return default;
            }
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            try
            {
                await EnsureInitializedAsync();
                if (!_isInitialized)
                    return;

                await _localStorage.SetItemAsync(key, value);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                // Silenciosamente falha durante pré-renderização
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar item no localStorage: {ex.Message}");
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await EnsureInitializedAsync();
                if (!_isInitialized)
                    return;

                await _localStorage.RemoveItemAsync(key);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                // Silenciosamente falha durante pré-renderização
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao remover item do localStorage: {ex.Message}");
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                await EnsureInitializedAsync();
                if (!_isInitialized)
                    return;

                await _localStorage.ClearAsync();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                // Silenciosamente falha durante pré-renderização
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao limpar localStorage: {ex.Message}");
            }
        }
    }
}
