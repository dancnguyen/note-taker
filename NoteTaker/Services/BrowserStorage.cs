using System.Text.Json;
using Microsoft.JSInterop;

namespace NoteTaker.Services
{
  public class BrowserStorage : IStorage
  {
    private readonly IJSRuntime jsRuntime;

    public BrowserStorage(IJSRuntime jsRuntime)
    {
      this.jsRuntime = jsRuntime;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
      var json = jsRuntime is IJSInProcessRuntime inProcess ? inProcess.Invoke<string?>("localStorage.getItem", key) : await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

      if (json is null) 
        return default;

      try
      {
        return JsonSerializer.Deserialize<T>(json);
      }
      catch (JsonException)
      {
        return default;
      }
    }

    public async Task SetAsync<T>(string key, T value)
    {
      var json = JsonSerializer.Serialize(value);

      if (jsRuntime is IJSInProcessRuntime inProcess)
      {
        inProcess.InvokeVoid("localStorage.setItem", key, json);
        return;
      }

      await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveAsync(string key)
    {
      if (jsRuntime is IJSInProcessRuntime inProcess)
      {
        inProcess.InvokeVoid("localStorage.removeItem", key);
        return;
      }

      await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
  }
}
