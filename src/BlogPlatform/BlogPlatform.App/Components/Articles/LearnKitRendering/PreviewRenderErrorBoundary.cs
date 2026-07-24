using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlogPlatform.App.Components.Articles.LearnKitRendering;

public sealed class PreviewRenderErrorBoundary : ErrorBoundary
{
    [Parameter]
    public EventCallback<Exception> OnError { get; set; }

    protected override async Task OnErrorAsync(Exception exception)
    {
        if (OnError.HasDelegate)
        {
            await OnError.InvokeAsync(exception);
        }
    }
}
