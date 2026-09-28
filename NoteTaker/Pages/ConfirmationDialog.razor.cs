using Microsoft.AspNetCore.Components;
using Radzen;

namespace NoteTaker.Pages
{
  public partial class ConfirmationDialog
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

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public string ConfirmText { get; set; } = "Confirm";

    [Parameter]
    public string CancelText { get; set; } = "Cancel";

    protected void OnConfirm() => DialogService.Close(true);

    protected void OnCancel() => DialogService.Close(false);

  }
}
