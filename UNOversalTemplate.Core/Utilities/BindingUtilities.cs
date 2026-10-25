using System;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

// Binding lifecycle methods for XAML Pages on .NET 10+.
// All operations are no-ops because the XAML binding engine auto-updates when DataContext changes,
// eliminating runtime reflection on XAML internals under AOT/trimming.
#if NET10_0_OR_GREATER

namespace UNOversal
{
    /// <summary>
    /// Binding lifecycle methods for XAML Pages (.NET 10+).
    /// </summary>
    public static class BindingUtilities
    {
        public static void UpdateBindings(this Page page)
        {
            /* no-op: modern XAML updates bindings automatically */
        }

        public static void InitializeBindings(this Page page)
        {
            /* no-op: modern XAML initializes bindings automatically */
        }

        public static void StopTrackingBindings(this Page page)
        {
            /* no-op: modern XAML manages tracking automatically */
        }
    }
}
#else
// Pre-.NET 10 path: reflection-based binding manipulation.
using System.Reflection;
using UNOversal.Services.Logging;
using UNOversal.Ioc;



namespace UNOversal
{
    public static class BindingUtilities
    {
        /// <summary>
        /// Updates bindings via reflection on the internal XAML Bindings field.
        /// </summary>
        public static void UpdateBindings(this Page page)
        {
            try
            {
                if (page == null) return;
#pragma warning disable IL2075, IL2072 -- Unmanaged calling convention reflection on XAML internals
                var field = page.GetType().GetTypeInfo().GetDeclaredField("Bindings");
                var bindings = field?.GetValue(page);
                var method = bindings?.GetType().GetMethod("Update", Type.EmptyTypes);
#pragma warning restore IL2075, IL2072
                method?.Invoke(bindings, Array.Empty<object>());
            }
            catch
            {
                try
                {
                    var logger = UNOversalApplicationBase.Current.Container.Resolve<ILoggingService>();
                    logger.LogException(new Exception("BindingUtilities.UpdateBindings failed"), LoggingPreferEnum.Simple);
                }
                catch { /* suppress during shutdown */ }
            }
        }

        /// <summary>
        /// Initializes bindings via reflection on the internal XAML Bindings field.
        /// </summary>
        public static void InitializeBindings(this Page page)
        {
            try
            {
                if (page == null) return;
#pragma warning disable IL2075, IL2072
                var field = page.GetType().GetTypeInfo().GetDeclaredField("Bindings");
                var bindings = field?.GetValue(page);
#pragma warning restore IL2075, IL2072
                var method = bindings?.GetType().GetMethod("Initialize", Type.EmptyTypes);
                method?.Invoke(bindings, Array.Empty<object>());
            }
            catch { /* best effort */ }
        }

        /// <summary>
        /// Stops tracking bindings via reflection on the internal XAML Bindings field.
        /// </summary>
        public static void StopTrackingBindings(this Page page)
        {
            try
            {
                if (page == null) return;
#pragma warning disable IL2075, IL2072
                var field = page.GetType().GetTypeInfo().GetDeclaredField("Bindings");
                var bindings = field?.GetValue(page);
#pragma warning restore IL2075, IL2072
                var method = bindings?.GetType().GetMethod("StopTracking", Type.EmptyTypes);
                method?.Invoke(bindings, Array.Empty<object>());
            }
            catch { /* best effort */ }
        }
    }
}
#endif
