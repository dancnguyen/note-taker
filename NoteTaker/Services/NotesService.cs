using Microsoft.JSInterop;
using NoteTaker.Models;

namespace NoteTaker.Services
{
  public class NotesService
  {
    private const string NotesStorageKey = "notes";
    private const string CurrentNoteStorageKey = "currentNote";

    private static readonly TimeSpan SaveDelay = TimeSpan.FromMilliseconds(300);

    private readonly IStorage storage;
    private readonly List<Note> notes = new();
    private bool loaded;
    private CancellationTokenSource? pendingSave;

    public NotesService(IStorage storage)
    {
      this.storage = storage;
      Current = AddNoteToList();
    }

    public IReadOnlyList<Note> Notes => notes;

    public Note Current { get; private set; }

    public event Action? CurrentChanged;

    public async Task LoadNotesAsync()
    {
      if (loaded) 
        return;

      loaded = true;

      List<string>? savedContents;
      int savedCurrent;
      try
      {
        savedContents = await storage.GetItemAsync<List<string>>(NotesStorageKey);
        savedCurrent = await storage.GetItemAsync<int>(CurrentNoteStorageKey);
      }
      catch (JSException)
      {
        return;
      }

      if (savedContents is null || savedContents.Count == 0) 
        return;

      notes.Clear();
      foreach (var content in savedContents) 
        AddNoteToList().Content = content ?? string.Empty;

      Current = notes.FirstOrDefault(n => n.Number == savedCurrent) ?? notes[0];
      CurrentChanged?.Invoke();
    }

    public async Task AddNoteAsync()
    {
      await SelectNoteAsync(AddNoteToList());
      await SaveNotesAsync();
    }

    public async Task SelectNoteAsync(Note note)
    {
      if (note == Current) 
        return;

      Current = note;
      CurrentChanged?.Invoke();
      await SaveAsync(CurrentNoteStorageKey, Current.Number);
    }

    public async Task ClearAllAsync()
    {
      CancelPendingSave();

      notes.Clear();
      Current = AddNoteToList();
      CurrentChanged?.Invoke();

      try
      {
        await storage.RemoveItemAsync(NotesStorageKey);
        await storage.RemoveItemAsync(CurrentNoteStorageKey);
      }
      catch (JSException) { }
    }

    public async Task UpdateCurrentContentAsync(string content)
    {
      Current.Content = content;

      CancelPendingSave();
      var save = pendingSave = new CancellationTokenSource();
      try
      {
        await Task.Delay(SaveDelay, save.Token);
      }
      catch (TaskCanceledException)
      {
        return;
      }

      await SaveNotesAsync();
    }

    private Note AddNoteToList()
    {
      var note = new Note(notes.Count + 1);
      notes.Add(note);
      return note;
    }

    private Task SaveNotesAsync()
    {
      CancelPendingSave();
      return SaveAsync(NotesStorageKey, notes.Select(n => n.Content).ToList());
    }

    [JSInvokable]
    public void FlushPendingSave()
    {
      if (pendingSave is null)
      {
        return;
      }

      _ = SaveNotesAsync();
    }

    private void CancelPendingSave()
    {
      pendingSave?.Cancel();
      pendingSave?.Dispose();
      pendingSave = null;
    }

    private async Task SaveAsync<T>(string key, T value)
    {
      try
      {
        await storage.SetItemAsync(key, value);
      }
      catch (JSException) { }
    }
  }
}
