using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace UNOversal
{
    public static class XamlUtilities
    {
        public static List<FrameworkElement> VisualChildren(this DependencyObject parent)
        {
            return RecurseChildren(parent);
        }

        public static List<FrameworkElement> RecurseChildren(DependencyObject parent)
        {
            var list = new List<FrameworkElement>();
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element)
                {
                    list.Add(element);
                }
                list.AddRange(RecurseChildren(child));
            }
            return list;
        }

    }
}
