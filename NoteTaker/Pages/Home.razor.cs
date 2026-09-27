using Microsoft.AspNetCore.Components;
using Radzen;

namespace NoteTaker.Pages
{
  public partial class Home
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

    protected async Task OnConfirmClick()
    {
      await DialogService.OpenAsync<ConfirmationDialog>("Confirm", new Dictionary<string, object?>(), new DialogOptions());
    }

  }
}
