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

      List<SavedNote>? savedNotes;
      int savedCurrent;
      try
      {
        savedNotes = await storage.GetItemAsync<List<SavedNote>>(NotesStorageKey);
        savedCurrent = await storage.GetItemAsync<int>(CurrentNoteStorageKey);

        savedNotes ??= (await storage.GetItemAsync<List<string>>(NotesStorageKey))?.Select((content, i) => new SavedNote(i + 1, content)).ToList();
      }
      catch (JSException)
      {
        return;
      }

      if (savedNotes is null || savedNotes.Count == 0) 
        return;

      notes.Clear();
      foreach (var saved in savedNotes)
      {
        var number = saved.Number > 0 && notes.All(n => n.Number != saved.Number) ? saved.Number : NextNumber();
        notes.Add(new Note(number) { Content = saved.Content ?? string.Empty });
      }

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

    public async Task DeleteNoteAsync(Note note)
    {
      var index = notes.IndexOf(note);
      if (index < 0)
        return;

      if (notes.Count == 1)
      {
        await ClearAllAsync();
        return;
      }

      notes.RemoveAt(index);

      if (note == Current)
      {
        Current = notes[Math.Min(index, notes.Count - 1)];
        await SaveAsync(CurrentNoteStorageKey, Current.Number);
      }

      CurrentChanged?.Invoke();
      await SaveNotesAsync();
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
      var note = new Note(NextNumber());
      notes.Add(note);
      return note;
    }

    private int NextNumber() => notes.Count == 0 ? 1 : notes.Max(n => n.Number) + 1;

    private Task SaveNotesAsync()
    {
      CancelPendingSave();
      return SaveAsync(NotesStorageKey, notes.Select(n => new SavedNote(n.Number, n.Content)).ToList());
    }

    [JSInvokable]
    public void FlushPendingSave()
    {
      if (pendingSave is null) 
        return;

      _ = SaveNotesAsync();
    }

    private void CancelPendingSave()
    {
      pendingSave?.Cancel();
      pendingSave?.Dispose();
      pendingSave = null;
    }

    private record SavedNote(int Number, string Content);

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
