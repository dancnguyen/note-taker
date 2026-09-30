namespace NoteTaker.Models
{
  public class ExportRequest
  {
    public ExportRequest(string fileName, ExportFormat format)
    {
      FileName = fileName;
      Format = format;
    }

    public string FileName { get; }

    public ExportFormat Format { get; }

    public string FullFileName => FileName.EndsWith(Format.Extension, StringComparison.OrdinalIgnoreCase) ? FileName : FileName + Format.Extension;
  }
}
