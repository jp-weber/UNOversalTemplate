using System;
using System.Linq;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif
using Windows.Foundation;

namespace UNOversal.Navigation
{
    public partial class NavigationPath : INavigationPath
    {
        private readonly string _originalString;

        /// <summary>
        /// The resolved Page type used by native Frame navigation and retained for history metadata.
        /// </summary>
        public Type View { get; }

        /// <summary>
        /// Associated ViewModel type for auto-wiring.
        /// </summary>
        public Type ViewModel { get; }

        /// <summary>
        /// Activator captured at registration time and used to restore pages from navigation history.
        /// </summary>
        public Func<Page> Activator { get; }

        public string QueryString { get; }

        public NavigationPath(int index, string originalString, INavigationParameters parameters)
        {
            Index = index;

            _originalString = originalString;

            // parse name/key

            Key = originalString.Split('?').First();

            // parse query

            var queryString = originalString.Split('?').Last();
            if (queryString != Key)
            {
                QueryString = queryString;
            }

            // parse parameters

            if (!string.IsNullOrEmpty(QueryString))
            {
                var query = new WwwFormUrlDecoder(QueryString);
                foreach (var item in query)
                {
                    Parameters.Add(item.Name, item.Value);
                }
            }

            // merge parameters

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    Parameters.Add(item.Key, item.Value);
                }
            }

            // get types from PageNavigationRegistry

            if (PageNavigationRegistry.TryGetRegistration(Key, out var info))
            {
                Key = info.Key;
                View = info.View;
                ViewModel = info.ViewModel;
                Activator = info.Activator;  // Capture AOT-safe activator delegate if available
            }
            else
            {
                throw new Exception($"Count not find Key:[{Key}]. Use RegisterTypes()/container.RegisterForNavigation<T, T>().");
            }
        }

        public int Index { get; }

        public INavigationParameters Parameters { get; } = new NavigationParameters();

        public string Key { get; }

        public override string ToString()
        {
            return _originalString;
        }
    }
}
