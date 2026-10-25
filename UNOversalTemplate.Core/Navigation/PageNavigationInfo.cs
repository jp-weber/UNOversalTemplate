using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using WinRT;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Navigation
{
    /// <summary>
    /// Holds navigation info for a registered page, including an optional activator for history restoration.
    /// </summary>
    public class PageNavigationInfo
    {
        public string Key { get; set; }

        /// <summary>
        /// The page type used by native Frame navigation and retained for history metadata.
        /// </summary>
        public Type View { get; set; }

        /// <summary>
        /// Associated ViewModel type for auto-wiring (if any).
        /// </summary>
        public Type ViewModel { get; set; }

        /// <summary>
        /// Page factory delegate captured at registration time and used when restoring history entries.
        /// Null when registered via non-generic RegisterForNavigation(Type, string) overload.
        /// </summary>
        public Func<Page> Activator { get; internal set; }

        /// <summary>
        /// Creates a page instance using the registered activator or a reflection fallback.
        /// </summary>
        [DynamicWindowsRuntimeCast(typeof(Page))]
        public Page CreatePage()
        {
            if (Activator != null)
                return Activator.Invoke();

            if (View != null)
                return System.Activator.CreateInstance(View) as Page;

            return null;
        }
    }
}
