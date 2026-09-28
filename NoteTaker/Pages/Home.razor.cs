using Microsoft.AspNetCore.Components;
using NoteTaker.Services;
using Radzen;

namespace NoteTaker.Pages
{
  public partial class Home : IDisposable, IHandleEvent
  {
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    [Inject]
    protected NotificationService NotificationService { get; set; } = default!;

    [Inject]
    protected ContextMenuService ContextMenuService { get; set; } = default!;

    [Inject]
    protected TooltipService TooltipService { get; set; } = default!;

    [Inject]
    protected NotesService NotesService { get; set; } = default!;

    protected override void OnInitialized()
    {
      NotesService.CurrentChanged += StateHasChanged;
    }

    protected Task OnContentInput(string content)
    {
      return NotesService.UpdateCurrentContentAsync(content);
    }

    Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
    {
      return callback.InvokeAsync(arg);
    }

    public void Dispose()
    {
      NotesService.CurrentChanged -= StateHasChanged;
    }
  }
}
