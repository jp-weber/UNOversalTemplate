using System;
using System.Linq;
using Prism.Common;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using System.Threading.Tasks;
using UNOversal.Logging;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;
using UNOversal.Ioc;
using UNOversal.Modularity;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using WinUIWindow = Windows.UI.Xaml.Window;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WinUIWindow = Microsoft.UI.Xaml.Window;
using Uno.Extensions.Maui.Platform;
#endif

namespace UNOversal
{
    /// <summary>
    /// Base application class that provides a basic initialization sequence
    /// </summary>
    /// <remarks>
    /// This class must be overridden to provide application specific configuration.
    /// </remarks>
#if !MAUI_EMBEDDING
    public abstract partial class UNOversalApplicationBase : Application
#else
    public abstract partial class UNOversalApplicationBase : EmbeddingApplication
#endif
    {
#if !MAUI_EMBEDDING
        public static new UNOversalApplicationBase Current => (UNOversalApplicationBase)Application.Current;
#else
        public static new UNOversalApplicationBase Current => (UNOversalApplicationBase)EmbeddingApplication.Current;
#endif
        private static readonly SemaphoreSlim _startSemaphore = new SemaphoreSlim(1, 1);
        private readonly bool _logStartingEvents = false;
        IContainerExtension _containerExtension;
        IModuleCatalog _moduleCatalog;
        private static int _initialized = 0;
        private ILoggerFacade _logger;

        public UNOversalApplicationBase()
        {
            InternalInitialize();
            _logger.Log("[App.Constructor()]", Category.Info, Priority.None);

            CoreApplication.Exiting += (s, e) =>
            {
                StopArgs stopArgs = new StopArgs(StopKind.CoreApplicationExiting) { CoreApplicationEventArgs = e };
                OnStop(stopArgs);
                OnStopAsync(stopArgs);
            };
        }

        public Func<SplashScreen, UIElement> ExtendedSplashScreenFactory { get; set; }
        /// <summary>
        /// The dependency injection container used to resolve objects
        /// </summary>
        public IContainerProvider Container => _containerExtension;

        /// <summary>
        /// Run the initialization process.
        /// </summary>
        private void InternalInitialize()
        {
            // don't forget there is no logger yet
            if (_logStartingEvents)
            {
                _logger.Log($"{nameof(UNOversalApplicationBase)}.{nameof(InternalInitialize)}", Category.Info, Priority.None);
            }

            // dependecy injection
            ContainerLocator.SetContainerExtension(CreateContainerExtension);
            Debug.WriteLine("[App.RegisterTypes()]");
            _containerExtension = ContainerLocator.Current;
            _moduleCatalog = CreateModuleCatalog();
            RegisterRequiredTypes(_containerExtension);
            RegisterTypes(_containerExtension);

            if (_containerExtension is IContainerRegistry registry)
            {
                registry.RegisterSingleton<ILoggerFacade, DebugLogger>();
                registry.RegisterSingleton<IEventAggregator, EventAggregator>();
                RegisterInternalTypes(registry);
            }
            Debug.WriteLine("Dependency container has just been finalized.");
            _containerExtension.FinalizeExtension();

            ConfigureModuleCatalog(_moduleCatalog);

            //var regionAdapterMappings = _containerExtension.Resolve<RegionAdapterMappings>();
            //ConfigureRegionAdapterMappings(regionAdapterMappings);

            //var defaultRegionBehaviors = _containerExtension.Resolve<IRegionBehaviorFactory>();
            //ConfigureDefaultRegionBehaviors(defaultRegionBehaviors);

            RegisterFrameworkExceptionTypes();

            // now we can start logging instead of debug/write
            _logger = Container.Resolve<ILoggerFacade>();

            // finalize the application
            ConfigureViewModelLocator();
        }

        private static int _started = 0;

        private async Task InternalStartAsync(ApplicationArgs startArgs)
        {
            await _startSemaphore.WaitAsync();
            if (_logStartingEvents)
            {
                _logger.Log($"{nameof(UNOversalApplicationBase)}.{nameof(InternalStartAsync)}({startArgs})", Category.Info, Priority.None);
            }

            // sometimes activation is rased through the base.onlaunch. We'll fix that.
            if (Interlocked.Increment(ref _started) > 1 && startArgs.StartKind == StartKinds.Launch)
            {
                startArgs.StartKind = StartKinds.Activate;
            }

            //SetupExtendedSplashScreen();

            try
            {
                CallOnInitializedOnlyOnce();
                //var suspensionUtil = new SuspensionUtilities();
                //if (suspensionUtil.IsResuming(startArgs, out var resumeArgs))
                //{
                //    startArgs.StartKind = StartKinds.ResumeFromTerminate;
                //    startArgs.Arguments = resumeArgs;
                //}
                //suspensionUtil.ClearSuspendDate();

                _logger.Log($"[App.OnStart(startKind:{startArgs.StartKind}, startCause:{startArgs.StartCause})]", Category.Info, Priority.None);
                OnStart(startArgs);

                _logger.Log($"[App.OnStartAsync(startKind:{startArgs.StartKind}, startCause:{startArgs.StartCause})]", Category.Info, Priority.None);
                await OnStartAsync(startArgs);
            }
            finally
            {
                _startSemaphore.Release();
            }

            //void SetupExtendedSplashScreen()
            //{
            //    if (startArgs.StartKind == StartKinds.Launch
            //        && startArgs.Arguments is IActivatedEventArgs act
            //        && WinUIWindow.Current.Content is null
            //        && (ExtendedSplashScreenFactory != null))
            //    {
            //        try
            //        {
            //            WinUIWindow.Current.Content = ExtendedSplashScreenFactory(act.SplashScreen);
            //        }
            //        catch (Exception ex)
            //        {
            //            throw new Exception($"Error during {nameof(ExtendedSplashScreenFactory)}.", ex);
            //        }
            //    }
            //}
        }

        private void CallOnInitializedOnlyOnce()
        {
            // don't forget there is no logger yet
            if (_logStartingEvents)
            {
                _logger.Log($"{nameof(UNOversalApplicationBase)}.{nameof(CallOnInitializedOnlyOnce)}", Category.Info, Priority.None);
            }

            // once and only once, ever
            if (Interlocked.Increment(ref _initialized) == 1)
            {
                _logger.Log("[App.OnInitialize()]", Category.Info, Priority.None);
                UIElement shell = CreateShell();
                if (shell != null)
                {
                    //MvvmHelpers.AutowireViewModel(shell);
                    //InitializeShell(shell);

                    void FinalizeInitialization()
                    {
                        //RegionManager.SetRegionManager(shell, _containerExtension.Resolve<IRegionManager>());
                        //RegionManager.UpdateRegions();

                        InitializeModules();
                    }

                    if (shell is FrameworkElement fe)
                    {
                        void OnLoaded(object sender, object args)
                        {
                            FinalizeInitialization();
                            fe.Loaded -= OnLoaded;
                        }

                        // We need to delay the initialization after the shell has been loaded, otherwise 
                        // the visual tree is not materialized for the RegionManager to be available.
                        // See https://github.com/PrismLibrary/Prism/issues/2102 for more details.
                        FinalizeInitialization();
                        fe.Loaded -= OnLoaded;
                        fe.Loaded += OnLoaded;
                    }
                    else
                    {
                        FinalizeInitialization();
                    }
                }

                //InitializeModules();
                //OnInitialized();
            }
        }

        #region overrides
        public virtual void OnStop(IStopArgs stopArgs) { /* empty */ }

        public virtual Task OnStopAsync(IStopArgs stopArgs)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// Creates the container used by Prism.
        /// </summary>
        /// <returns>The container</returns>
        protected abstract IContainerExtension CreateContainerExtension();

        /// <summary>
        /// Creates the <see cref="IModuleCatalog"/> used by Prism.
        /// </summary>
        ///  <remarks>
        /// The base implementation returns a new ModuleCatalog.
        /// </remarks>
        protected virtual IModuleCatalog CreateModuleCatalog()
        {
            return new ModuleCatalog();
        }

        /// <summary>
        /// Registers all types that are required by Prism to function with the container.
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected virtual void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterRequiredTypes(_moduleCatalog);
        }

        /// <summary>
        /// Used to register types with the container that will be used by your application.
        /// </summary>
        public abstract void RegisterTypes(IContainerRegistry containerRegistry);

        public virtual void ConfigureViewModelLocator()
        {
            // this is a testability method
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {
                return _containerExtension.ResolveViewModelForView(view, type);
            });
        }

        /// <summary>
        /// Configures the <see cref="IRegionBehaviorFactory"/>. 
        /// This will be the list of default behaviors that will be added to a region. 
        /// </summary>
        //protected virtual void ConfigureDefaultRegionBehaviors(IRegionBehaviorFactory regionBehaviors)
        //{
        //    regionBehaviors?.RegisterDefaultRegionBehaviors();
        //}

        ///// <summary>
        ///// Configures the default region adapter mappings to use in the application, in order
        ///// to adapt UI controls defined in XAML to use a region and register it automatically.
        ///// May be overwritten in a derived class to add specific mappings required by the application.
        ///// </summary>
        ///// <returns>The <see cref="RegionAdapterMappings"/> instance containing all the mappings.</returns>
        //protected virtual void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        //{
        //    regionAdapterMappings?.RegisterDefaultRegionAdapterMappings();
        //}

        /// <summary>
        /// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
        /// root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected virtual void RegisterFrameworkExceptionTypes()
        {
        }

        /// <summary>
        /// Creates the shell or main window of the application.
        /// </summary>
        /// <returns>The shell of the application.</returns>
        protected abstract UIElement CreateShell();

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        protected virtual void InitializeShell(UIElement shell)
        {
            WinUIWindow.Current.Content = shell;

            // Activate must be called immediately in order for the Loaded event to be raised
            // in the shell.
            WinUIWindow.Current.Activate();
        }


        public virtual void OnStart(IApplicationArgs args) {  /* empty */ }

        public virtual Task OnStartAsync(IApplicationArgs args)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Configures the <see cref="IModuleCatalog"/> used by Prism.
        /// </summary>
        protected virtual void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) { }

        /// <summary>
        /// Initializes the modules.
        /// </summary>
        protected virtual void InitializeModules()
        {
            //PrismInitializationExtensions.RunModuleManager(Container);
        }

        protected virtual void RegisterInternalTypes(IContainerRegistry containerRegistry)
        {
            // don't forget there is no logger yet
            Debug.WriteLine($"{nameof(UNOversalApplicationBase)}.{nameof(RegisterInternalTypes)}()");
        }
        #endregion
    }
}
