
using System.Threading;
using Windows.Foundation;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Services.Dialogs
{
    public static class DialogExtensions
    {
        public static IAsyncOperation<ContentDialogResult> ShowAsync(this ContentDialog dialog, CancellationToken token)
        {
            token.Register(() => dialog.Hide());
            return dialog.ShowAsync();
        }
    }
}
