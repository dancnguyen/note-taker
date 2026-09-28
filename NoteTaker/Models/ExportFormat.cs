namespace NoteTaker.Models
{
  public class ExportFormat
  {
    public static readonly IReadOnlyList<ExportFormat> All = new[]
    {
      new ExportFormat("Plain Text", ".txt", "text/plain"),
      new ExportFormat("Markdown", ".md", "text/markdown"),
      new ExportFormat("HTML", ".html", "text/html")
    };

    public ExportFormat(string name, string extension, string mimeType)
    {
      Name = name;
      Extension = extension;
      MimeType = mimeType;
    }

    public string Name { get; }

    public string Extension { get; }

    public string MimeType { get; }

    public string Label => $"{Name} ({Extension})";
  }
}
