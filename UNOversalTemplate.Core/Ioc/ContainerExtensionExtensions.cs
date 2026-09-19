using System;
using UNOversal.Navigation;
using UNOversal.Mvvm;
using WinRT;


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
                if (page.Frame != null)
                {
                    INavigationService service = NavigationService.Instances[page.Frame];
                    return extension.Resolve(viewModelType, (typeof(INavigationService), service));
                }
                else
                {
                    return extension.Resolve(viewModelType);
                }
            }
            else
            {
                return extension.Resolve(viewModelType);
            }
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
        /// Registers an object for navigation
        /// </summary>
        /// <param name="containerRegistry"></param>
        /// <param name="type">The type of object to register</param>
        /// <param name="name">The unique name to register with the object.</param>
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
        public static void RegisterForNavigation<TView, TViewModel>(this IContainerRegistry containerRegistry, string name = null)
        {
            containerRegistry.RegisterForNavigationWithViewModel<TViewModel>(typeof(TView), name);
        }

        private static void RegisterForNavigationWithViewModel<TViewModel>(this IContainerRegistry containerRegistry, Type viewType, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = viewType.Name;

            ViewModelLocationProvider.Register(viewType.ToString(), typeof(TViewModel));

            // Register the ViewModel type with DI so constructor injection works correctly.
            // This is essential when ViewModels have dependencies (e.g., INavigationService, IEventAggregator).
            // Without this, Resolve() falls back to Activator.CreateInstance which fails 
            // if there's no parameterless constructor.
            containerRegistry.Register(typeof(TViewModel), typeof(TViewModel));

            containerRegistry.RegisterForNavigation(viewType, name);
        }
    }
}
