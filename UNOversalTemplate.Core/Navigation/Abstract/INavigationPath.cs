using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Navigation
{
    public interface INavigationPath
    {
        int Index { get; }
        string Key { get; }

        /// <summary>
        /// The resolved page type used by native Frame navigation and retained for history metadata.
        /// </summary>
        Type View { get; }

        /// <summary>
        /// Associated ViewModel type for auto-wiring.
        /// </summary>
        Type ViewModel { get; }

        /// <summary>
        /// Activator captured at registration time and used to restore pages from navigation history.
        /// </summary>
        Func<Page> Activator { get; }

        INavigationParameters Parameters { get; }
        string QueryString { get; }

        string ToString();
    }
}