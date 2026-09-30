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
        string? savedTheme = await Storage.GetItemAsync<string>(ThemeStorageKey);
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

    public void CloseSidebar()
    {
      if (!SidebarExpanded)
        return;

      SidebarExpanded = false;
      StateHasChanged();
    }

    protected async Task OnNoteClickAsync(Note note)
    {
      await NotesService.SelectNoteAsync(note);
      SidebarExpanded = false;
    }

    protected async Task OnAddNoteClickAsync()
    {
      await NotesService.AddNoteAsync();
      SidebarExpanded = false;
    }

    protected bool CanDeleteNote(Note note) => NotesService.Notes.Count > 1 || note.Number != 1;

    protected async Task OnDeleteNoteClickAsync(Note note)
    {
      dynamic? confirmed = await DialogService.OpenAsync<ConfirmationDialog>("Confirm", new Dictionary<string, object?> { ["Message"] = $"Are you sure you would like to delete {note.Name}?" });

      if (!confirmed)
        return;

      var wasLastNote = NotesService.Notes.Count == 1;
      await NotesService.DeleteNoteAsync(note);

      if (wasLastNote)
        SidebarExpanded = false;
    }

    protected async Task OnClearAllClickAsync()
    {
      var confirmed = await DialogService.OpenAsync<ConfirmationDialog>("Confirm", new Dictionary<string, object?> { ["Message"] = "Are you sure you would like to delete all notes?" });

      if (!confirmed)
        return;

      await NotesService.ClearAllAsync();
      SidebarExpanded = false;
    }

    protected async Task OnExportClickAsync()
    {
      Note note = NotesService.Current;
      if (await DialogService.OpenAsync<ExportDialog>("Export Note", new Dictionary<string, object?> { ["FileName"] = $"Note-{note.Number}" }) is not ExportRequest export)
        return;

      SidebarExpanded = false;

      string result;
      try
      {
        result = await JSRuntime.InvokeAsync<string>("noteTaker.exportNote", note.Name, note.Content, export.Format, export.FullFileName);
      }
      catch (JSException ex)
      {
        NotificationService.Notify(NotificationSeverity.Error, "Export failed", ex.Message);
        return;
      }

      if (result == "saved")
        NotificationService.Notify(NotificationSeverity.Success, "Exported", $"{note.Name} was saved as {export.FullFileName}.");
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
