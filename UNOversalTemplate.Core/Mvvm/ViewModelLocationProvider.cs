using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using UNOversal.Ioc;

namespace UNOversal.Mvvm
{
    /// <summary>
    /// The ViewModelLocationProvider class locates the view model for the view that has the AutoWireViewModelChanged attached property set to true.
    /// The view model will be located and injected into the view's DataContext. To locate the view model, two strategies are used: First the ViewModelLocationProvider
    /// will look to see if there is a view model factory registered for that view, if not it will try to infer the view model using a convention based approach.
    /// This class also provides methods for registering the view model factories,
    /// and also to override the default view model factory and the default view type to view model type resolver.
    /// </summary>

    // Documentation on using the MVVM pattern is at http://go.microsoft.com/fwlink/?LinkID=288814&clcid=0x409

    public static class ViewModelLocationProvider
    {
        /// <summary>
        /// A dictionary that contains all the registered factories for the views.
        /// </summary>
        static Dictionary<string, Func<object>> _factories = new Dictionary<string, Func<object>>();

        /// <summary>
        /// A dictionary that contains all the registered ViewModel types for the views.
        /// Key is the string-registered view key (e.g., from GetViewKey below). Value is null if explicitly registered as "no VM".
        /// </summary>
        static Dictionary<string, Type?> _typeFactories = new Dictionary<string, Type?>();

        /// <summary>
        /// Tracks views that were explicitly registered without a ViewModel.
        /// Stores short names (e.g., "ShellPage") for matching when GetType() works correctly.
        /// </summary>
        static HashSet<string> _explicitlyRegisteredNoVmNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The most recently navigated-to view name, set by the navigation system before AutoWireViewModelChanged is triggered.
        /// Used for reliable ViewModel lookup under AOT/trimming where GetType() may return incorrect base types (e.g., "Page" instead of "ShellPage").
        /// </summary>
        public static string? CurrentNavigatedViewName { get; internal set; }

        /// <summary>
        /// Clears the navigated view name after autowire processing completes.
        /// Prevents stale references across multiple concurrent navigations.
        /// </summary>
        internal static void ClearCurrentNavigatedViewName()
        {
            CurrentNavigatedViewName = null;
        }

        /// <summary>
        /// The default view model factory which provides the ViewModel type as a parameter.
        /// </summary>
        static Func<Type, object> _defaultViewModelFactory = CreateViewModelInstance;

        /// <summary>
        /// ViewModelFactory that provides the View instance and ViewModel type as parameters.
        /// </summary>
        static Func<object, Type, object> _defaultViewModelFactoryWithViewParameter;

        /// <summary>
        /// Default view type to view model type resolver, assumes the view model is in same assembly as the view type, but in the "ViewModels" namespace.
        /// Convention-based resolution uses assembly-qualified type names which cannot be statically analyzed by the trimmer.
        /// Register view models explicitly via <see cref="Register{T,VM}"/> or <see cref="Register{T}(Func{object})"/> to avoid trimming issues.
        /// </summary>
#if NET10_0_OR_GREATER
        [UnconditionalSuppressMessage("Trimming", "IL2057:Type.GetType",
            Justification = "Convention-based VM resolver uses assembly-qualified names. Register VMs explicitly for AOT/trimming safety.")]
#endif
        static Func<Type, Type?> _defaultViewTypeToViewModelTypeResolver =
            viewType =>
            {
                var viewName = viewType.FullName;
                viewName = viewName!.Replace(".Views.", ".ViewModels.");
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
                var viewModelName = String.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
                return Type.GetType(viewModelName);
            };

        /// <summary>
        /// Default factory implementation with trimming annotation so the parameterless constructor is preserved.
        /// </summary>
        private static object CreateViewModelInstance(
#if WINDOWS_UWP && NET10_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] 
#endif
            Type type)
            => Activator.CreateInstance(type)!;

        /// <summary>
        /// Sets the default view model factory.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory which provides the ViewModel type as a parameter.</param>
        public static void SetDefaultViewModelFactory(Func<Type, object> viewModelFactory)
        {
            _defaultViewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Sets the default view model factory.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory that provides the View instance and ViewModel type as parameters.</param>
        public static void SetDefaultViewModelFactory(Func<object, Type, object> viewModelFactory)
        {
            _defaultViewModelFactoryWithViewParameter = viewModelFactory;
        }

        /// <summary>
        /// Sets the default view type to view model type resolver.
        /// </summary>
        /// <param name="viewTypeToViewModelTypeResolver">The view type to view model type resolver.</param>
        public static void SetDefaultViewTypeToViewModelTypeResolver(Func<Type, Type> viewTypeToViewModelTypeResolver)
        {
            _defaultViewTypeToViewModelTypeResolver = viewTypeToViewModelTypeResolver;
        }

        /// <summary>
        /// Automatically looks up the viewmodel that corresponds to the current view, using two strategies:
        /// It first looks to see if there is a mapping registered for that view, if not it will fallback to the convention based approach.
        /// </summary>
        /// <param name="view">The dependency object, typically a view.</param>
        /// <param name="setDataContextCallback">The call back to use to create the binding between the View and ViewModel</param>
        public static void AutoWireViewModelChanged(object view, Action<object, object> setDataContextCallback)
        {
            // Try registered mappings/factories first.
            object viewModel = GetViewModelForView(view);

            if (viewModel != null)
            {
                setDataContextCallback(view, viewModel);
                return;
            }

            var viewKey = view.GetType().FullName ?? view.GetType().ToString();

            var viewModelType = GetViewModelTypeForView(view.GetType());

            // Fallback to convention-based resolution ONLY for pre-.NET 10 targets.
            // Under .NET 10 the convention uses assembly-qualified names via Type.GetType() 
            // which cannot be statically analyzed by the linker — VM types may be trimmed away 
            // at runtime, leading to silent failures that are hard to diagnose.
#if !NET10_0_OR_GREATER
            if (viewModelType == null)
                viewModelType = _defaultViewTypeToViewModelTypeResolver(view.GetType());
#endif

            if (viewModelType == null)
            {
                var logger = UNOversalApplicationBase.Current?.Container.Resolve<UNOversal.Logging.ILoggerFacade>();
#if NET10_0_OR_GREATER
                // Only warn if the view was NOT explicitly registered as "no VM".
                // Under .NET 10, GetType() may return wrong base types due to AOT stripping,
                // so we use explicit name tracking for reliable matching.
                bool hasExplicitNoVmMapping = IsViewExplicitlyRegisteredWithoutViewModel(view);
                if (!hasExplicitNoVmMapping)
                {
                    var viewName = view.GetType().FullName ?? view.GetType().ToString();
                    logger?.Log(
                        $"[UNOversal] ViewModel not registered for '{viewName}'. " +
                        "Convention-based resolution is disabled under .NET 10 (AOT/trimming incompatible). " +
                        "Register explicitly via ViewModelLocationProvider.Register<T, VM>().",
                        UNOversal.Logging.Category.Warn, UNOversal.Logging.Priority.None);
                }
#else
                logger?.Log($"[UNOversal] No ViewModel found for '{view.GetType()}' via explicit registration or convention.",
                        UNOversal.Logging.Category.Warn, UNOversal.Logging.Priority.None);
#endif
                return;
            }

            viewModel = _defaultViewModelFactoryWithViewParameter != null
                ? _defaultViewModelFactoryWithViewParameter(view, viewModelType)
                : _defaultViewModelFactory(viewModelType);

            setDataContextCallback(view, viewModel);
        }

        /// <summary>
        /// Gets the view model for the specified view.
        /// </summary>
        /// <param name="view">The view that the view model wants.</param>
        /// <returns>The ViewModel that corresponds to the view passed as a parameter.</returns>
        private static object GetViewModelForView(object view)
        {
            var viewKey = view.GetType().ToString();

            // Mapping of view models base on view type (or instance) goes here
            if (_factories.ContainsKey(viewKey))
                return _factories[viewKey]();

            return null;
        }

        /// <summary>
        /// Gets the ViewModel type for the specified view.
        /// Uses registered mappings first, then falls back to CurrentNavigatedViewName when GetType() is unreliable under AOT.
        /// </summary>
        private static Type? GetViewModelTypeForView(Type view)
        {
            var viewFullName = view.FullName;

            // 1st: Direct lookup by full name (works when GetType() is reliable).
            if (!string.IsNullOrEmpty(viewFullName) && _typeFactories.TryGetValue(viewFullName, out Type? vm))
                return vm;

            string viewNameOnly = view.Name;

            // 2nd: Try matching registered keys that END with our view's short name.
            foreach (var registeredKey in _typeFactories.Keys)
            {
                if (registeredKey.EndsWith(viewNameOnly, StringComparison.Ordinal))
                    return _typeFactories[registeredKey];
            }

            // 3rd: AOT fallback — use CurrentNavigatedViewName set by the navigation system.
            // When GetType() returns "Page" instead of "ShellPage", this lets us find 
            // the correct registered key (e.g., "UWP_Sample.Views.ShellPage") because it ends with "ShellPage".
            if (!string.IsNullOrEmpty(CurrentNavigatedViewName))
            {
                foreach (var registeredKey in _typeFactories.Keys)
                {
                    if (registeredKey.EndsWith(CurrentNavigatedViewName, StringComparison.Ordinal))
                        return _typeFactories[registeredKey];
                }
            }

            return null;
        }

        /// <summary>
        /// Registers the ViewModel factory for the specified view type.
        /// </summary>
        /// <typeparam name="T">The View</typeparam>
        /// <param name="factory">The ViewModel factory.</param>
        public static void Register<T>(Func<object> factory)
        {
            Register(typeof(T).ToString(), factory);
        }

        /// <summary>
        /// Registers the ViewModel factory for the specified view type name.
        /// </summary>
        /// <param name="viewTypeName">The name of the view type.</param>
        /// <param name="factory">The ViewModel factory.</param>
        public static void Register(string viewTypeName, Func<object> factory)
        {
            _factories[viewTypeName] = factory;
        }

        /// <summary>
        /// Registers a ViewModel type for the specified view type.
        /// Prefer this over convention-based resolution when targeting trimmed or AOT-compiled builds.
        /// </summary>
        /// <typeparam name="T">The View</typeparam>
        /// <typeparam name="VM">The ViewModel</typeparam>
#if NET10_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(object))] // kept generic; concrete types preserved via typeof(VM)
#endif
        public static void Register<T,
#if NET10_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] 
#endif
            VM>()
        {
            var viewType = typeof(T);
            var viewModelType = typeof(VM);

            Register(viewType.ToString(), viewModelType);
        }

        /// <summary>
        /// Registers a ViewModel type for the specified view.
        /// </summary>
        /// <param name="viewTypeName">The View type name</param>
        /// <param name="viewModelType">The ViewModel type (null to mark as "no VM")</param>
        public static void Register(string viewTypeName, Type? viewModelType)
        {
            _typeFactories[viewTypeName] = viewModelType;
            
            // Track explicit no-VM registrations by short name for matching.
            if (viewModelType == null)
            {
                var lastDot = viewTypeName.LastIndexOf('.');
                var shortName = lastDot >= 0 ? viewTypeName.Substring(lastDot + 1) : viewTypeName;
                _explicitlyRegisteredNoVmNames.Add(shortName);
            }
        }

        /// <summary>
        /// Returns true if the specified view was explicitly registered as "no ViewModel".
        /// Uses CurrentNavigatedViewName as primary matching under AOT.
        /// </summary>
        public static bool IsViewExplicitlyRegisteredWithoutViewModel(object view)
        {
            // Phase 1: If we have the navigated name, match against it (most reliable).
            if (!string.IsNullOrEmpty(CurrentNavigatedViewName))
            {
                foreach (var key in _typeFactories.Keys)
                {
                    if (_typeFactories[key] == null && key.EndsWith(CurrentNavigatedViewName, StringComparison.Ordinal))
                        return true;
                }

                var vn = view.GetType().Name;
                if (_explicitlyRegisteredNoVmNames.Contains(vn))
                {
                    bool hasVmRegistration = false;
                    foreach (var k in _typeFactories.Keys)
                    {
                        if (_typeFactories[k] != null && k.EndsWith(vn, StringComparison.Ordinal))
                        {
                            hasVmRegistration = true;
                            break;
                        }
                    }
                    if (!hasVmRegistration) return true;
                }
            }

            // Phase 2: Fallback to view type name matching when no navigation context exists.
            string? viewNameOnly = view.GetType().Name;
            if (_explicitlyRegisteredNoVmNames.Contains(viewNameOnly))
                return true;

            var viewFullName = view.GetType().FullName;
            if (!string.IsNullOrEmpty(viewFullName))
            {
                foreach (var key in _typeFactories.Keys)
                {
                    if (_typeFactories[key] == null && key.EndsWith(viewNameOnly, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        static string GetViewTypeName(object view)
        {
            var type = view.GetType();
            var fullName = type.FullName;
            if (!string.IsNullOrEmpty(fullName))
            {
                var dot = fullName.LastIndexOf('.');
                return dot >= 0 ? fullName.Substring(dot + 1) : fullName;
            }
            return type.ToString()!;
        }
    }
}