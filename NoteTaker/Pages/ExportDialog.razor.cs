using Microsoft.AspNetCore.Components;
using NoteTaker.Models;
using Radzen;

namespace NoteTaker.Pages
{
  public partial class ExportDialog
  {
    [Inject]
    protected DialogService DialogService { get; set; } = default!;

    protected ExportFormat SelectedFormat { get; set; } = ExportFormat.All[0];

    protected void OnExport() => DialogService.Close(SelectedFormat);

    protected void OnCancel() => DialogService.Close(null);
  }
}
