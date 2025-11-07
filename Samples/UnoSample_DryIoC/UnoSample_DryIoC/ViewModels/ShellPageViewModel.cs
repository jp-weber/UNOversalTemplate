using Microsoft.UI.Xaml.Data;
using UNOversal.Navigation;

namespace UNO_Sample.ViewModels
{
#if !WINDOWS_UWP
        [Bindable]
#endif
        public class ShellPageViewModel : ObservableRecipient
        {
            private bool _navigationIsAllowed = true;
            private string _title;
            private bool _isScreenCaptureEnabled;
            public ICommand AccountCodePageCommand { get; }
            public ICommand SearchPageCommand { get; }
            public ICommand SettingsPageCommand { get; }
#if !WINDOWS_UWP
            //private TabBarItem _selectedItem;
            private int _selectedIndex = 0;
            private bool _isMobileSearchActive = false;
#endif

            public ShellPageViewModel()
            {
#if ANDROID || IOS
                //AccountCodePageCommand = new AsyncRelayCommand(AccountCodePageCommandTask);
                //SearchPageCommand = new AsyncRelayCommand(SearchPageCommandTask);
                //SettingsPageCommand = new AsyncRelayCommand(SettingsPageCommandTask);
#endif
            }

#if __ANDROID__ || __IOS__
            //private async Task SettingsPageCommandTask()
            //{
            //    if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            //    {
            //        if (uIElement is not SettingPage)
            //        {
            //            await NavigationService.NavigateAsync(nameof(SettingPage));
            //        }
            //    }
            //}

            //private async Task SearchPageCommandTask()
            //{
            //    if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            //    {
            //        if (uIElement is SettingPage)
            //        {
            //            await NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
            //        }
            //    }
            //}

            //private async Task AccountCodePageCommandTask()
            //{
            //    if (App.ShellPageInstance.MainFrame.Content is UIElement uIElement)
            //    {
            //        if (uIElement is not AccountCodePage)
            //        {
            //            await NavigationService.NavigateAsync("/" + nameof(AccountCodePage));
            //        }
            //    }
            //}
#endif

            public INavigationService NavigationService { get; internal set; }
            public bool NavigationIsAllowed
            {
                get => _navigationIsAllowed;
                set
                {
                    if (SetProperty(ref _navigationIsAllowed, value))
                    {
#if ANDROID || IOS
                        OnPropertyChanged(nameof(IsMobile));
#endif
                    }
                }
            }
            public string Title
            {
                get => _title;
                set => SetProperty(ref _title, value);
            }


#if !WINDOWS_UWP

            private bool _tabBarIsVisible;
            /// <summary>
            /// The TabBar should be only visible at mobile devices
            /// </summary>
            public bool IsMobile
            {
                get
                {
#if ANDROID || IOS
                    return true;
#else
                return false;
#endif
                }
            }
            public bool TabBarIsVisible
            {
                get => IsMobile & _tabBarIsVisible;
                set => SetProperty(ref _tabBarIsVisible, value);
            }


            //public int SelectedIndex
            //{
            //    get => _selectedIndex;
            //    set
            //    {
            //        SetProperty(ref _selectedIndex, value);
            //        //only for mobile devices, check if account filters are set

            //        if (value == 0)
            //        {
            //            if (DataService.Instance.ACVCollection.Filter != null)
            //            {
            //                DataService.Instance.ACVCollection.Filter = null;
            //            }
            //        }

            //        if (value == 1)
            //        {
            //            IsMobileSearchActive = true;
            //        }
            //        else
            //        {
            //            IsMobileSearchActive = false;
            //        }
            //    }
            //}

            //public TabBarItem SelectedItem
            //{
            //    get => _selectedItem;
            //    set => SetProperty(ref _selectedItem, value);
            //}
            public bool IsMobileSearchActive
            {
                get => _isMobileSearchActive;
                set => SetProperty(ref _isMobileSearchActive, value);
            }


            public Thickness GetTabBarMargin
            {
                get
                {
#if IOS
                return new Thickness(0, 0, 0, 30);
#else
                    return new Thickness(0, 0, 0, 4);
#endif
                }
            }
#endif
        }
}
