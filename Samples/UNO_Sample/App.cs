using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Prism.Ioc;
using System;
using System.Threading.Tasks;
using UNO_Sample.Views;
using UNOversal;
using UNOversal.DryIoc;
using UNOversal.Services.Dialogs;
using UNOversal.Services.File;
using UNOversal.Services.Secrets;
using UNOversal.Services.Settings;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace UNO_Sample
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : UNOversalApplication
    {
        /// <summary>
        /// Creates the access of the static instance of the ShellPage
        /// </summary>
        public static ShellPage ShellPageInstance { get; private set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// <remarks>
        /// If you're looking for App.xaml.cs, the file is present in each platform head
        /// of the solution.
        /// </remarks>
        public App()
        {
        }

        /// <summary>
        /// Gets the main window of the app.
        /// </summary>
        internal static Window MainWindow { get; private set; }

        public override async Task OnStartAsync(IApplicationArgs args)
        {
            App.Current = MainWindow ?? new Window();
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IDialogService, DialogService>();
            containerRegistry.RegisterSingleton<IFileService, FileService>();
            containerRegistry.RegisterSingleton<ISecretService, SecretService>();
            containerRegistry.RegisterSingleton<ISettingsHelper, SettingsHelper>();
        }

        protected override UIElement CreateShell()
        {
            ShellPageInstance = new ShellPage();
            return ShellPageInstance;
        }
    }
}