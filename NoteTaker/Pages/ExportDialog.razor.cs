using Microsoft.AspNetCore.Components;
using NoteTaker.Models;
using Radzen;

namespace NoteTaker.Pages
{
  public partial class ExportDialog
  {
    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    [Parameter]
    public string FileName { get; set; } = string.Empty;

    protected ExportFormat SelectedFormat { get; set; } = ExportFormat.All[0];

    protected bool CanExport => !string.IsNullOrWhiteSpace(FileName);

    protected void OnExport()
    {
      if (CanExport)
        DialogService.Close(new ExportRequest(FileName.Trim(), SelectedFormat));
    }

    protected void OnCancel() => DialogService.Close(null);
  }
}
