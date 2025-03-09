namespace RifaTech.UI.Shared.Services
{
    // RifaTech.UI.Shared/Services/ILocalStorageService.cs

    public interface IStorageService
    {
        Task<T> GetItemAsync<T>(string key);
        Task SetItemAsync<T>(string key, T value);
        Task RemoveItemAsync(string key);
        Task ClearAsync();
    }
}
