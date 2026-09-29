namespace NoteTaker.Services
{
  public interface IStorage
  {
    Task<T?> GetItemAsync<T>(string key);

    Task SetItemAsync<T>(string key, T value);

    Task RemoveItemAsync(string key);
  }
}
