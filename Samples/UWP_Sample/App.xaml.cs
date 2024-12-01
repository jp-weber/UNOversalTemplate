using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UNOversal;
using UNOversal.DryIoc;
using UNOversal.Ioc;
using UWP_Sample.ViewModels;
using UWP_Sample.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
        public static ShellPage ShellPageInstance { get; private set; } = new ShellPage();
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
                Window.Current.Activate();
            }
        }

        protected override UIElement CreateShell()
        {
            ShellPageInstance = Container.Resolve<ShellPage>();
            return ShellPageInstance;
        }

        public override void RegisterTypes(IContainerRegistry container)
        {
            // standard template 10 services
            //container.RegisterTemplate10Services();
            // custom services

            // pages and view-models
            container.RegisterSingleton<ShellPage, ShellPage>();
            container.RegisterForNavigation<MainPage, MainPageViewModel>();

            //contentdialogs and view-models

        }
    }
}
