using System.Threading.Tasks;
using UNOversal;
using UNOversal.Ioc;
using UWP_Sample.ViewModels;
using UWP_Sample.Views;
using Windows.UI.Xaml;

namespace UWP_Sample
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : UNOversalApplication
    {
        /// <summary>
        /// Creates the access of the static instance of the ShellPage
        /// </summary>
        public static ShellPage ShellPageInstance { get; private set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        public override async Task OnStartAsync(IApplicationArgs args)
        {
            if (Window.Current.Content == null)
            {
                Window.Current.Content = ShellPageInstance;
                await ShellPageInstance.NavigationService.NavigateAsync(nameof(MainPage));
                Window.Current.Activate();
            }
        }

        protected override UIElement CreateShell()
        {
            ShellPageInstance = new ShellPage();
            return ShellPageInstance;
        }

        public override void RegisterTypes(IContainerRegistry container)
        {
            // custom services

            // pages and view-models
            //container.RegisterSingleton<ShellPage, ShellPage>();
            container.RegisterForNavigation<MainPage, MainPageViewModel>();

            //contentdialogs and view-models

        }
    }
}
