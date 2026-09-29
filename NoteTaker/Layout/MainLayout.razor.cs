using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NoteTaker.Models;
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

    protected const string DefaultTheme = "material-dark";

    private const string ThemeStorageKey = "theme";

    private DotNetObjectReference<NotesService>? NotesServiceReference;

    protected override async Task OnInitializedAsync()
    {
      try
      {
        var savedTheme = await Storage.GetItemAsync<string>(ThemeStorageKey);
        if (!string.IsNullOrEmpty(savedTheme)) 
          ThemeService.SetTheme(savedTheme);
      }
      catch (JSException) { }

      ThemeService.ThemeChanged += OnThemeChangedAsync;

      NotesService.CurrentChanged += StateHasChanged;
      await NotesService.LoadNotesAsync();

      NotesServiceReference = DotNetObjectReference.Create(NotesService);
      await JSRuntime.InvokeVoidAsync("noteTaker.saveOnPageHide", NotesServiceReference);
    }

    protected async Task OnClearAllClickAsync()
    {
      var confirmed = await DialogService.OpenAsync<ConfirmationDialog>("Confirm", new Dictionary<string, object?> { ["Message"] = "Are you sure you would like to delete all notes?" });

      if (confirmed is true) 
        await NotesService.ClearAllAsync();
    }

    protected async Task OnExportClickAsync()
    {
      if (await DialogService.OpenAsync<ExportDialog>("Export Note") is not ExportFormat format)
        return;

      var note = NotesService.Current;
      string result;
      try
      {
        result = await JSRuntime.InvokeAsync<string>("noteTaker.exportNote", note.Name, note.Content, format);
      }
      catch (JSException ex)
      {
        NotificationService.Notify(NotificationSeverity.Error, "Export failed", ex.Message);
        return;
      }

      if (result == "saved")
        NotificationService.Notify(NotificationSeverity.Success, "Exported", $"{note.Name} was saved as {format.Name}.");
    }

    private async void OnThemeChangedAsync()
    {
      try
      {
        await Storage.SetItemAsync(ThemeStorageKey, ThemeService.Theme);
      }
      catch (JSException) { }
    }

    public void Dispose()
    {
      ThemeService.ThemeChanged -= OnThemeChangedAsync;
      NotesService.CurrentChanged -= StateHasChanged;
      NotesServiceReference?.Dispose();
    }
  }
}
