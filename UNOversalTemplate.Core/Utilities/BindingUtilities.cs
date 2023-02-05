﻿using System;
using System.Reflection;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal
{
    public static class BindingUtilities
    {
        public static void UpdateBindings(this Page page)
        {
            if (page == null)
            {
                return;
            }
            var field = page.GetType().GetTypeInfo().GetDeclaredField("Bindings");
            var bindings = field?.GetValue(page);
            var update = bindings?.GetType().GetRuntimeMethod("Update", Array.Empty<Type>());
            update?.Invoke(bindings, null);
        }

        public static void InitializeBindings(this Page page)
        {
            if (page == null)
            {
                return;
            }
            var field = page.GetType().GetTypeInfo().GetDeclaredField("Bindings");
            var bindings = field?.GetValue(page);
            var update = bindings?.GetType().GetRuntimeMethod("Initialize", Array.Empty<Type>());
            update?.Invoke(bindings, null);
        }

        public static void StopTrackingBindings(this Page page)
        {
            if (page == null)
            {
                return;
            }
            var field = page.GetType().GetTypeInfo().GetDeclaredField("Bindings");
            var bindings = field?.GetValue(page);
            var update = bindings?.GetType().GetRuntimeMethod("StopTracking", Array.Empty<Type>());
            update?.Invoke(bindings, null);
        }
    }
}
