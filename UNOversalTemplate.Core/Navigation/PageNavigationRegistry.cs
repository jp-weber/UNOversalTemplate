using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Navigation
{
    public static partial class PageNavigationRegistry
    {
        static Dictionary<string, PageNavigationInfo> _pageRegistrationCache = new Dictionary<string, PageNavigationInfo>();
        
        /// <summary>
        /// Stores AOT-safe VM activator delegates keyed by the view type's fully qualified name.
        /// Used for non-DI resolution to avoid reflection-based Activator.CreateInstance under DisableRuntimeMarshalled=true.
        /// </summary>
        static Dictionary<string, Func<object>> _vmActivators = new Dictionary<string, Func<object>>();

        /// <summary>
        /// Registers a page activator and preserves the actual page type for history restoration and autowiring.
        /// </summary>
        public static void RegisterAotSafe(string key, Type viewType, Func<Page> activator, Type viewModelType = null)
        {
            var info = new PageNavigationInfo
            {
                Key = key,
                Activator = activator,
                ViewModel = viewModelType ?? typeof(object),
                View = viewType ?? typeof(Page)
            };

            if (!_pageRegistrationCache.ContainsKey(key))
                _pageRegistrationCache.Add(key, info);
        }

        /// <summary>
        /// Legacy overload retained for compatibility with older callers.
        /// </summary>
        public static void RegisterAotSafe(string key, Func<Page> activator, Type viewModelType = null)
        {
            RegisterAotSafe(key, typeof(Page), activator, viewModelType);
        }

        public static void Register(string key, (Type View, Type ViewModel) infoTuple)
        {
            var info = new PageNavigationInfo
            {
                Key = key,
                View = infoTuple.View,
                ViewModel = infoTuple.ViewModel
            };

            if (!_pageRegistrationCache.ContainsKey(key))
                _pageRegistrationCache.Add(key, info);
        }

        public static bool TryGetRegistration(string key, out PageNavigationInfo info)
        {
            if (_pageRegistrationCache.ContainsKey(key))
            {
                info = new PageNavigationInfo
                {
                    Key = key,
                    View = _pageRegistrationCache[key].View,
                    ViewModel = _pageRegistrationCache[key].ViewModel,
                    Activator = _pageRegistrationCache[key].Activator
                };
                return true;
            }
            else
            {
                info = new PageNavigationInfo
                {
                    Key = null,
                    View = null,
                    ViewModel = null
                };
                return false;
            }
        }

        public static bool TryGetRegistration(Type view, out PageNavigationInfo info)
        {
            if (_pageRegistrationCache.Any(x => x.Value.View == view))
            {
                var cache = _pageRegistrationCache.FirstOrDefault(x => x.Value.View == view);
                info = new PageNavigationInfo
                {
                    Key = cache.Key,
                    View = view,
                    ViewModel = cache.Value.ViewModel,
                    Activator = cache.Value.Activator
                };
                return true;
            }
            else if (TryGetRegistration(view.Name, out info))
            {
                return true;
            }
            else
            {
                info = new PageNavigationInfo
                {
                    Key = null,
                    View = null,
                    ViewModel = null
                };
                return false;
            }
        }

        public static PageNavigationInfo GetPageNavigationInfo(string name)
        {
            if (_pageRegistrationCache.ContainsKey(name))
                return _pageRegistrationCache[name];

            return null;
        }

        public static PageNavigationInfo GetPageNavigationInfo(Type pageType)
        {
            foreach (var item in _pageRegistrationCache)
            {
                if (item.Value.View == pageType)
                    return item.Value;
            }

            return null;
        }

        public static Type GetPageType(string name)
        {
            return GetPageNavigationInfo(name)?.View;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ClearRegistrationCache()
        {
            _pageRegistrationCache.Clear();
        }

        /// <summary>
        /// Registers an AOT-safe VM activator delegate for a view type, keyed by the ViewModel's FullName.
        /// </summary>
        /// <param name="vmActivator">The delegate that creates the ViewModel instance.</param>
        /// <param name="viewModelFullName">The fully qualified Name of the ViewModel type (used as the key).</param>
        public static void RegisterAotSafe(Func<object> vmActivator, string viewTypeName)
        {
            if (!_vmActivators.ContainsKey(viewTypeName))
                _vmActivators.Add(viewTypeName, vmActivator);
        }

        /// <summary>
        /// Tries to get an AOT-safe VM activator delegate for a given ViewModel type.
        /// Returns true and sets the activator if found; otherwise returns false.
        /// </summary>
        public static bool TryGetVmActivator(Type viewModelType, out Func<object> activator)
        {
            // Direct lookup by ViewModel FullName (registered via RegisterForNavigationAotSafe<TView, TViewModel>)
            if (_vmActivators.ContainsKey(viewModelType.FullName))
            {
                activator = _vmActivators[viewModelType.FullName];
                return true;
            }

            activator = null;
            return false;
        }

        /// <summary>
        /// Tries to get an AOT-safe VM activator by view type name (e.g., "MyNamespace.MyPage").
        /// </summary>
        public static bool TryGetVmActivator(string viewTypeName, out Func<object> activator)
        {
            if (_vmActivators.ContainsKey(viewTypeName))
            {
                activator = _vmActivators[viewTypeName];
                return true;
            }

            activator = null;
            return false;
        }
    }
}
