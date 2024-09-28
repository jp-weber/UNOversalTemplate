using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UNOversal.Navigation
{
    public static partial class PageNavigationRegistry
    {
        static Dictionary<string, PageNavigationInfo> _pageRegistrationCache = new Dictionary<string, PageNavigationInfo>();

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
                    ViewModel = _pageRegistrationCache[key].ViewModel
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
                    ViewModel = cache.Value.ViewModel
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
    }
}
