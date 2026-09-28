using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NoteTaker.Pages;
using NoteTaker.Services;
using Radzen;

namespace NoteTaker.Layout
{
  public partial class MainLayout : IDisposable
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
    protected ThemeService ThemeService { get; set; } = default!;

    [Inject]
    protected IStorage Storage { get; set; } = default!;

    [Inject]
    protected NotesService NotesService { get; set; } = default!;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    protected bool SidebarExpanded { get; set; }

    private DotNetObjectReference<NotesService>? notesServiceReference;

    protected const string DefaultTheme = "material-dark";

    private const string ThemeStorageKey = "theme";

    protected override async Task OnInitializedAsync()
    {
      try
      {
        var savedTheme = await Storage.GetAsync<string>(ThemeStorageKey);
        if (!string.IsNullOrEmpty(savedTheme))
        {
          ThemeService.SetTheme(savedTheme);
        }
      }
      catch (JSException) { }

      ThemeService.ThemeChanged += OnThemeChanged;

      NotesService.CurrentChanged += StateHasChanged;
      await NotesService.LoadAsync();

      notesServiceReference = DotNetObjectReference.Create(NotesService);
      await JSRuntime.InvokeVoidAsync("noteTaker.saveOnPageHide", notesServiceReference);
    }

    protected async Task OnClearAllClick()
    {
      var confirmed = await DialogService.OpenAsync<ConfirmationDialog>("Confirm",
        new Dictionary<string, object?> { ["Message"] = "Are you sure you would like to delete all notes?" });

      if (confirmed is true) 
        await NotesService.ClearAllAsync();
    }

    private async void OnThemeChanged()
    {
      try
      {
        await Storage.SetAsync(ThemeStorageKey, ThemeService.Theme);
      }
      catch (JSException) { }
    }

    public void Dispose()
    {
      ThemeService.ThemeChanged -= OnThemeChanged;
      NotesService.CurrentChanged -= StateHasChanged;
      notesServiceReference?.Dispose();
    }
  }
}
