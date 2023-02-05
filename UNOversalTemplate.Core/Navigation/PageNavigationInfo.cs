using System;

namespace UNOversal.Navigation
{
    public class PageNavigationInfo
    {
        public string Key { get; set; }

        public Type View { get; set; }

        public Type ViewModel { get; set; }
    }
}
