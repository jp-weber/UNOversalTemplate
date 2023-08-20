using System;
using System.Threading;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Services.Dialogs
{
    /// <summary>
    /// Interface to show modal and non-modal dialogs.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog, IDialogParameters parameters, TimeSpan? timeout = null, CancellationToken? token = null);

        /// <summary>
        /// closes all dialogs where no user defined token was passed
        /// </summary>
        void CloseDialogs();

        /// <summary>
        /// Checks if a Dialog is running
        /// </summary>
        /// <returns>true if a dialog was started by this service</returns>
        Task<bool> IsDialogRunning();
    }
}
