using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UNO_Sample.ViewModels;
using UNOversal.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace UnoSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        public NavigationView ShellViewInternal { get; private set; }

        public ShellPageViewModel ViewModel { get; } = new ShellPageViewModel();

        public Frame MainFrame { get; }

        public ShellPage()
        {
            this.InitializeComponent();

            ShellViewInternal = ShellView;
            ShellView.Content = MainFrame = new Frame();
            ViewModel.NavigationService = NavigationFactory.Create(MainFrame);
        }
    }
}
