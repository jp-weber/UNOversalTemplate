using System;
using UNOversal.Navigation;
using UNOversal.Mvvm;
#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif
using System.Collections.Concurrent;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Ioc
{
    /// <summary>
    /// <see cref="IContainerRegistry"/> extensions.
    /// </summary>
    public static partial class ContainerExtensionExtensions
    {

        private static readonly ConcurrentDictionary<Type, Func<object>> _viewModelFactories = new();


        public static void RegisterForNavigation<TView, TViewModel>(this IContainerRegistry containerRegistry, string name = null) 
            where TView : Page

        {

            var serviceName = name ?? typeof(TView).Name;

            // AOT-safe delegate capture: Capture the resolved factory at registration time
            // This ensures the generic type information is preserved for AOT compilation
            var viewType = typeof(TView);
            var viewModelType = typeof(TViewModel);

#if NET10_0_OR_GREATER
            // Get the service provider and capture a factory delegate
            // This approach avoids reflective Resolve<T>() which fails under AOT
            var serviceProvider = ((IContainerExtension<IServiceProvider>)containerRegistry).Instance;

            if (serviceProvider != null)
            {
                // Create a strongly-typed factory delegate that captures the service provider
                // The delegate is created in the generic context, so AOT compiler has full type info
                Func<object> factory = CreateViewModelFactory(viewModelType, serviceProvider);
                _viewModelFactories[viewType] = factory;
            }
            else
            {
                // Fallback for registration time before provider is built
                // Defer factory creation until first resolution
                Func<object> factory = () =>
                {
                    var sp = ((IContainerExtension<IServiceProvider>)containerRegistry).Instance;
                    return sp.GetService(viewModelType) ?? Activator.CreateInstance(viewModelType);
                };
                _viewModelFactories[viewType] = factory;
            }
#else
            _viewModelFactories[viewType] = () => containerRegistry.Resolve<TViewModel>();
#endif

            // Register in the navigation registry for type lookup
            PageNavigationRegistry.Register(serviceName, (viewType, viewModelType));

            System.Diagnostics.Debug.WriteLine($"[AOT] ViewModel für '{serviceName}' registriert (AOT-Safe).");
        }

        /// <summary>
        /// Creates an AOT-safe factory for a ViewModel type.
        /// This method captures the service provider in a closure.
        /// </summary>
        private static Func<object> CreateViewModelFactory(Type viewModelType, IServiceProvider serviceProvider)
        {
            // Capture the service provider and type in a closure
            // This is AOT-safe as it doesn't use reflective generics
            return () =>
            {
                try
                {
                    // Try to resolve from the service provider
                    var instance = serviceProvider.GetService(viewModelType);
                    if (instance != null)
                    {
                        return instance;
                    }

                    // Fallback: Create instance using default constructor (AOT-safe)
                    return Activator.CreateInstance(viewModelType);
                }
                catch
                {
                    // Last resort: try to create with default constructor
                    try
                    {
                        return Activator.CreateInstance(viewModelType);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AOT] Failed to create ViewModel {viewModelType.Name}: {ex.Message}");
                        return null;
                    }
                }
            };
        }


#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Page))]
#endif
        public static object ResolveViewModelForView(this IContainerExtension containerExtension, object view, Type viewModelType = null)
        {
            // Extension method overload for IContainerExtension
            // This version accepts optional viewModelType for flexibility
            return ResolveViewModelForView(view);
        }

#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Page))]
#endif
        public static object ResolveViewModelForView(object view)
        {
            // Wir ermitteln den Typ des übergebenen Objekts direkt (typ-sicher für Dictionary Lookup).
            // Dies ist AOT-sicher, da nur die Objektinstance gegeben ist, und wir keine Reflection auf dem Typ durchführen.

            if (view is Page page && page != null && _viewModelFactories.TryGetValue(view.GetType(), out var resolve))
            {
                // Aufruf des gespeicherten Func-delegates! Kein new() per Reflection mehr.
                return resolve();
            }

            return null;
        }


#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Page))]
#endif
        public static bool TryGetViewModelForView(object view)
        {
            if (view is Page page && page != null)
            {
                return _viewModelFactories.ContainsKey(page.GetType());
            }
            return false;
        }

        /// <summary>
        /// Registers an object to be used as a dialog in the IDialogService.
        /// </summary>
        /// <typeparam name="TView">The Type of object to register as the dialog</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register with the dialog.</param>
        public static void RegisterDialog<TView>(this IContainerRegistry containerRegistry, string name = null)
        {
            containerRegistry.RegisterForNavigation<TView>(name);
        }

        /// <summary>
        /// Registers a Transient with the given service and mapping to the specified implementation <see cref="Type" />.
        /// </summary>
        /// <param name="from">The service <see cref="Type" /></param>
        /// <param name="to">The implementation <see cref="Type" /></param>
        /// <returns>The <see cref="IContainerRegistry" /> instance</returns>
        public static void RegisterMapping(this IContainerRegistry containerRegistry,Type from, Type to)
        {
            containerRegistry.RegisterMapping(from, to);
        }

        /// <summary>
        /// Registers an object to be used as a dialog in the IDialogService.
        /// </summary>
        /// <typeparam name="TView">The Type of object to register as the dialog</typeparam>
        /// <typeparam name="TViewModel">The ViewModel to use as the DataContext for the dialog</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register with the dialog.</param>
        public static void RegisterDialog<TView, TViewModel>(this IContainerRegistry containerRegistry, string name = null)
            where TView : Page
        {
            containerRegistry.RegisterForNavigation<TView, TViewModel>(name);
        }

        ///// <summary>
        ///// Registers an object that implements IDialogWindow to be used to host all dialogs in the IDialogService.
        ///// </summary>
        ///// <typeparam name="TWindow">The Type of the Window class that will be used to host dialogs in the IDialogService</typeparam>
        ///// <param name="containerRegistry"></param>
        //public static void RegisterDialogWindow<TWindow>(this IContainerRegistry containerRegistry) where TWindow : Services.Dialogs.IDialogWindow
        //{
        //    containerRegistry.Register(typeof(Services.Dialogs.IDialogWindow), typeof(TWindow));
        //}

        ///// <summary>
        ///// Registers an object that implements IDialogWindow to be used to host all dialogs in the IDialogService.
        ///// </summary>
        ///// <typeparam name="TWindow">The Type of the Window class that will be used to host dialogs in the IDialogService</typeparam>
        ///// <param name="containerRegistry"></param>
        ///// <param name="name">The name of the dialog window</param>
        //public static void RegisterDialogWindow<TWindow>(this IContainerRegistry containerRegistry, string name) where TWindow : Services.Dialogs.IDialogWindow
        //{
        //    containerRegistry.Register(typeof(Services.Dialogs.IDialogWindow), typeof(TWindow), name);
        //}

        /// <summary>
        /// Registers an object for navigation
        /// </summary>
        /// <param name="containerRegistry"></param>
        /// <param name="type">The type of object to register</param>
        /// <param name="name">The unique name to register with the object.</param>
        public static void RegisterForNavigation(this IContainerRegistry containerRegistry, Type type, string name)
        {
            // 1. Register in DI Container for object-key resolution
            containerRegistry.Register(typeof(object), type, name);

            // 2. CRITICAL: Also register in PageNavigationRegistry!
            // This is why NavigationService finds the page by key (e.g., "MainPage")
            //if (!PageNavigationRegistry.Exists(name))
            //{
            //    // We need to set the View type - but we only have the Type here
            //    // The full mapping with ViewModel happens in RegisterForNavigationWithViewModel
            //    PageNavigationRegistry.Register(name, (type, typeof(object)));
            //}
        }

        /// <summary>
        /// Registers an object for navigation.
        /// </summary>
        /// <typeparam name="T">The Type of the object to register as the view</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register with the object.</param>
        public static void RegisterForNavigation<T>(this IContainerRegistry containerRegistry, string name = null)
        {
            Type type = typeof(T);
            string viewName = string.IsNullOrWhiteSpace(name) ? type.Name : name;
            containerRegistry.RegisterForNavigation(type, viewName);
        }

        /// <summary>
        /// Registers an object for navigation with the ViewModel type to be used as the DataContext.
        /// </summary>
        /// <typeparam name="TView">The Type of object to register as the view</typeparam>
        /// <typeparam name="TViewModel">The ViewModel to use as the DataContext for the view</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register with the view</param>
        //public static void RegisterForNavigation<TView, TViewModel>(this IContainerRegistry containerRegistry, string name = null)
        //{
        //    containerRegistry.RegisterForNavigationWithViewModel<TViewModel>(typeof(TView), name);
        //}

        private static void RegisterForNavigationWithViewModel<TViewModel>(this IContainerRegistry containerRegistry, Type viewType, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = viewType.Name;

            ViewModelLocationProvider.Register(viewType.ToString(), typeof(TViewModel));
            PageNavigationRegistry.Register(viewType.Name, (viewType, typeof(TViewModel)));

            // Register the ViewModel in the DI container so it can be resolved later by ResolveViewModelForView.
            containerRegistry.Register(typeof(TViewModel));
            containerRegistry.RegisterForNavigation(viewType, name);
        }
    }
}
