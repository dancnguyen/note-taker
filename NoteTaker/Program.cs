using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NoteTaker.Services;
using Radzen;

namespace NoteTaker
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebAssemblyHostBuilder.CreateDefault(args);
      builder.RootComponents.Add<App>("#app");
      builder.RootComponents.Add<HeadOutlet>("head::after");

      builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

      builder.Services.AddRadzenComponents();
      builder.Services.AddScoped<IStorage, BrowserStorage>();
      builder.Services.AddScoped<NotesService>();

      await builder.Build().RunAsync();
    }
  }
}
