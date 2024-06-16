#if WINDOWS_UWP
using System;
using System.Collections.Generic;
using System.Linq;
using UNOversal;
using UNOversal.Services.Gesture;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace UNOversal.Services.WindowService
{
    
    public static class WindowService
    {
        public static Dictionary<Guid, Action<WindowCreatedEventArgs>> WindowCreatedCallBacks { get; } = new Dictionary<Guid, Action<WindowCreatedEventArgs>>();

        private static readonly List<Windows.UI.Xaml.Window> _instances = new List<Windows.UI.Xaml.Window>();

        public static void Register(Windows.UI.Xaml.Window window)
        {
            if (!_instances.Contains(window))
            {
                _instances.Add(window);
                window.Closed += Window_Closed;
            }
        }

        private static void Window_Closed(object sender, CoreWindowEventArgs e)
        {
            UnRegister(sender as Windows.UI.Xaml.Window);
        }

        private static void UnRegister(Windows.UI.Xaml.Window window)
        {
            window.Closed -= Window_Closed;
            _instances.Remove(window);
        }

        public static Windows.UI.Xaml.Window[] AllWindows => _instances.ToArray();

        public static bool TryGetWindow(FrameworkElement element, out Windows.UI.Xaml.Window window)
        {
            foreach (Windows.UI.Xaml.Window item in AllWindows)
            {
                if (item.Content is UIElement ui)
                {
                    if (ui == element)
                    {
                        window = item;
                        return true;
                    }
                    List<FrameworkElement> children = ui.VisualChildren();
                    if (children.Contains(element))
                    {
                        window = item;
                        return true;
                    }
                }
            }
            window = null;
            return false;
        }

        internal static void ForwardWindowCreated(WindowCreatedEventArgs args)
        {
            GestureService.SetupWindowListeners(args.Window.CoreWindow);
            foreach (KeyValuePair<Guid, Action<WindowCreatedEventArgs>> item in WindowCreatedCallBacks.ToArray())
            {
                item.Value?.Invoke(args);
            }
            _instances.Add(args.Window);
        }
    }
}
#endif
