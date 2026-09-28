namespace NoteTaker.Models
{
  public class Note
  {
    public Note(int number)
    {
      Number = number;
    }

    public int Number { get; }

    public string Name => $"Note {Number}";

    public string Content { get; set; } = string.Empty;
  }
}
