using Uno.Resizetizer;
using UNO_Sample.Views;
using WinUIWindow = Microsoft.UI.Xaml.Window;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;
using UNOversal;
using UNOversal.Ioc;
using UNOversal.DryIoc;
using UNOversal.Services.Serialization;
using Prism.Ioc;
using Uno.Extensions.Hosting;
using Windows.ApplicationModel.Activation;
using UNO_Sample.ViewModels;

namespace UNO_Sample;
public partial class App : UNOversalApplication
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    internal static ShellPage ShellPageInstance { get; private set; }
    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    public override async Task OnStartAsync(IApplicationArgs args)
    {
        if (MainWindow == null)
        {
            MainWindow = WinUIWindow.Current;
        }

        if (args?.Arguments is ILaunchActivatedEventArgs e)
        {

        }


#if DEBUG
        MainWindow.EnableHotReload();
#endif
        MainWindow.SetWindowIcon();


        if (MainWindow.Content == null)
        {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            MainWindow.Content = ShellPageInstance;
            await ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(LoginPage));
            //rootFrame.Navigate(typeof(MainPage), args.Arguments);
        }
        // Ensure the current window is active
        MainWindow.Activate();

    }

    public override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // pages and view-models
        containerRegistry.RegisterSingleton<ShellPage, ShellPage>();
        containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
        containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
        containerRegistry.RegisterForNavigation<BlankPage, BlankPageViewModel>();
    }

    protected override UIElement CreateShell()
    {
        ShellPageInstance = Container.Resolve<ShellPage>();
        return ShellPageInstance;
    }
}
