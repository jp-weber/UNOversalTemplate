using System;
using System.Linq;
using UNOversal.Navigation;
using UNOversal.Mvvm;
#if WINDOWS_UWP && NET10_0_OR_GREATER
using WinRT;
#endif

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
#if WINDOWS_UWP && NET10_0_OR_GREATER
        [DynamicWindowsRuntimeCast(typeof(Page))]
#endif
        public static object ResolveViewModelForView(this IContainerExtension extension, object view, Type viewModelType)
        {
            if (view is Page page)
            {
                INavigationService? navigationService = null;

                if (page.Frame != null && NavigationService.Instances.ContainsKey(page.Frame))
                {
                    // Frame is already initialized — use its scoped NavigationService
                    navigationService = NavigationService.Instances[page.Frame];
                }
                else
                {
                    // Try to find any registered NavigationService as fallback (e.g., during startup)
                    var serviceProvider = extension as IContainerExtension<IServiceProvider>;
                    if (serviceProvider?.Instance != null)
                    {
                        navigationService = serviceProvider.Instance.GetService(typeof(INavigationService)) as INavigationService;
                    }

                    // DI may return a dummy-instantiated INavigationService (with a new, never-used Frame).
                    // Prefer a NavigationService from an actual initialized Frame over the DI singleton.
                    if (NavigationService.Instances.Count > 0)
                    {
                        foreach (var pair in NavigationService.Instances)
                        {
                            if (pair.Key.Content != null || pair.Key.BackStack.Any() || pair.Key.ForwardStack.Any())
                            {
                                navigationService = pair.Value;
                                break;
                            }
                        }

                        // If no Frame with content found, fall back to the first registered one.
                        if (navigationService == null)
                        {
                            navigationService = NavigationService.Instances.Values.First();
                        }
                    }
                }

                if (navigationService != null)
                {
                    return extension.Resolve(viewModelType, (typeof(INavigationService), navigationService));
                }
            }

            return extension.Resolve(viewModelType);
        }

        /// <summary>
        /// Registers an object to be used as a dialog in the IDialogService.
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name"></param>
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
        public static void RegisterMapping(this IContainerRegistry containerRegistry, Type from, Type to)
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
        {
            containerRegistry.RegisterForNavigation<TView, TViewModel>(name);
        }

        /// <summary>
        /// Registers an object for navigation.
        /// </summary>
        /// <param name="containerRegistry"></param>
        /// <param name="type">The type of object to register</param>
        /// <param name="name">The unique name to register with the object.</param>
        [Obsolete("This overload does not capture a typed page activator. Use RegisterForNavigationAotSafe<T>() " +
                  "if an activator is needed for history restoration.")]
        public static void RegisterForNavigation(this IContainerRegistry containerRegistry, Type type, string name)
        {
            // 1. Register in DI Container for object-key resolution (e.g., Resolve<ShellPage>())
            containerRegistry.Register(typeof(object), type, name);

            // 2. CRITICAL: Also register in PageNavigationRegistry!
            // This is why NavigationService finds pages by key (e.g., "TutorialPage").
            // Without this, 'new NavigationPath(index, key)' throws at runtime.
            PageNavigationRegistry.Register(name, (type, typeof(object)));
        }

        /// <summary>
        /// Registers an object for navigation.
        /// </summary>
        /// <typeparam name="T">The Type of the object to register as the view</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register with the object.</param>
        [Obsolete("Use RegisterForNavigationAotSafe<T>() instead for AOT compatibility. " +
                  "This overload still works but does not capture an AOT-safe page activator delegate.")]
        public static void RegisterForNavigation<T>(this IContainerRegistry containerRegistry, string name = null)
        {
            Type type = typeof(T);
            string viewName = string.IsNullOrWhiteSpace(name) ? type.Name : name;
            containerRegistry.RegisterForNavigation(type, viewName);
        }

        /// <summary>
        /// Registers a page activator that captures the concrete Page type at compile time for history restoration.
        /// </summary>
        /// <typeparam name="TView">The concrete Page type (must have a public parameterless constructor)</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register the page with.</param>
        public static void RegisterForNavigationAotSafe<TView>(this IContainerRegistry containerRegistry, string name = null) 
            where TView : Page, new()
        {
            var viewName = string.IsNullOrWhiteSpace(name) ? typeof(TView).Name : name;

            // Capture the concrete type at compile time for creating pages during history restoration.
            Func<Page> activator = () => new TView();

            // Register in DI container for object-key resolution (e.g., Resolve<ShellPage>())
            containerRegistry.Register(typeof(object), typeof(TView), viewName);

            // Preserve the concrete page type in the registry metadata so history and autowire can resolve it correctly.
            PageNavigationRegistry.RegisterAotSafe(viewName, typeof(TView), activator, typeof(object));
        }

        /// <summary>
        /// Registers page and view-model activators using compile-time generic types.
        /// The generic type parameter TView ensures the concrete Page type is captured at compile time,
        /// The page and view-model activators support history restoration and view-model creation. Navigation
        /// itself uses Frame.Navigate and still requires WinRT TypeName marshalling.
        /// </summary>
        /// <typeparam name="TView">The concrete Page type (must have a public parameterless constructor)</typeparam>
        /// <typeparam name="TViewModel">The ViewModel type to use as DataContext</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register the page with.</param>
        public static void RegisterForNavigationAotSafe<TView, TViewModel>(this IContainerRegistry containerRegistry, string name = null) 
            where TView : Page, new()
            where TViewModel : class, new()
        {
            var viewName = string.IsNullOrWhiteSpace(name) ? typeof(TView).Name : name;

            // Capture the concrete type at compile time for creating pages during history restoration.
            Func<Page> activator = () => new TView();

            // Register ViewModel location for auto-wiring
            ViewModelLocationProvider.Register(typeof(TView).ToString(), typeof(TViewModel));

            // Create an AOT-safe VM activator using type parameters captured at compile time.
            // This avoids reflection-based Activator.CreateInstance which triggers WinRT TypeName 
            // conversion under DisableRuntimeMarshalled=true (error 0x80131500).
            Func<object> vmActivator = () => new TViewModel();

            // Register VM activator in PageNavigationRegistry keyed by the ViewModel's FullName.
            // This allows direct lookup without needing to trace back from the View type.
            PageNavigationRegistry.RegisterAotSafe(vmActivator, typeof(TViewModel).FullName);

            // Do NOT register ViewModel in DI - it would try to resolve constructor dependencies
            // like INavigationService which is frame-specific. ViewModels are created via 
            // the cached VM activator delegate (vmActivator), then NavigationService is set via 
            // property injection (ViewModelBase.NavigationService = _navigationService).

            // Register in DI container for object-key resolution (e.g., Resolve<ShellPage>())
            containerRegistry.Register(typeof(object), typeof(TView), viewName);

            // Preserve the concrete page type in the registry metadata; this keeps history/autowire logic sane while the page itself is created via a captured delegate.
            PageNavigationRegistry.RegisterAotSafe(viewName, typeof(TView), activator, typeof(TViewModel));
        }

        /// <summary>
        /// Registers a page with a custom factory delegate for DI-constructed pages.
        /// Use this when the page requires dependencies in its constructor that cannot be satisfied 
        /// by Activator.CreateInstance (e.g., via ViewModelLocator).
        /// </summary>
        /// <param name="containerRegistry"></param>
        /// <param name="viewType">The page type (for debugging and reflection fallback)</param>
        /// <param name="activator">A lambda that creates the page, e.g. () => MyContainer.Resolve&lt;MainPage&gt;()</param>
        /// <param name="name">The unique name to register with.</param>
        public static void RegisterForNavigationWithActivator(
            this IContainerRegistry containerRegistry, 
            Type viewType, 
            Func<Page> activator,
            string name = null)
        {
            var viewName = string.IsNullOrWhiteSpace(name) ? viewType.Name : name;

            // The activator delegate captures the concrete construction logic at registration time
            // — still avoids WinRT TypeName conversion because we use a captured closure not reflection
            containerRegistry.Register(typeof(object), viewType, viewName);
            PageNavigationRegistry.RegisterAotSafe(viewName, activator, typeof(object));
        }

        /// <summary>
        /// Registers an object for navigation with the ViewModel type to be used as the DataContext.
        /// </summary>
        /// <typeparam name="TView">The Type of object to register as the view</typeparam>
        /// <typeparam name="TViewModel">The ViewModel to use as the DataContext for the view</typeparam>
        /// <param name="containerRegistry"></param>
        /// <param name="name">The unique name to register with the view</param>
        public static void RegisterForNavigation<TView, TViewModel>(this IContainerRegistry containerRegistry, string name = null)
        {
            containerRegistry.RegisterForNavigationWithViewModel<TViewModel>(typeof(TView), name);
        }

        private static void RegisterForNavigationWithViewModel<TViewModel>(this IContainerRegistry containerRegistry, Type viewType, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = viewType.Name;

            ViewModelLocationProvider.Register(viewType.ToString(), typeof(TViewModel));

            // Do NOT register ViewModel in DI - it would try to resolve constructor dependencies
            // like INavigationService which is frame-specific. ViewModels are created via 
            // Activator.CreateInstance by ViewModelLocator, then NavigationService is set via 
            // property injection (ViewModelBase.NavigationService = _navigationService).
            
            containerRegistry.RegisterForNavigation(viewType, name);
        }

        /// <summary>
        /// Registers a page for navigation without a ViewModel. Use this for pages that don't
        /// need an automatic DataContext (e.g., shell/root pages created directly via constructor).
        /// </summary>
        public static void RegisterForNavigationNoViewModel(this IContainerRegistry containerRegistry, Type viewType, string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = viewType.Name;

            // Register in DI container for object-key resolution
            containerRegistry.Register(typeof(object), viewType, name);

            // Register with null VM to suppress convention-resolution warning under .NET 10+
            ViewModelLocationProvider.Register(viewType.ToString(), viewModelType: null);
        }

        /// <summary>
        /// Registers a page for navigation without a ViewModel. Use this for pages that don't
        /// need an automatic DataContext (e.g., shell/root pages created directly via constructor).
        /// </summary>
        public static void RegisterForNavigationNoViewModel<TView>(this IContainerRegistry containerRegistry, string name = null) 
            where TView : Page
        {
            containerRegistry.RegisterForNavigationNoViewModel(typeof(TView), name);
        }
    }
}
