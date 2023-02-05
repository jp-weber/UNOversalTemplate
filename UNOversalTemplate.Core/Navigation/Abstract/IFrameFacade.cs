using System;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.UI.Xaml.Media.Animation;
#else
using Microsoft.UI.Xaml.Media.Animation;
#endif


namespace UNOversal.Navigation
{
    public interface IFrameFacade
    {
        string Id { get; set; }

        bool CanGoBack();
        event EventHandler CanGoBackChanged;
        Task<INavigationResult> GoBackAsync(INavigationParameters parameters, NavigationTransitionInfo infoOverride);

        bool CanGoForward();
        event EventHandler CanGoForwardChanged;
        Task<INavigationResult> GoForwardAsync(INavigationParameters parameters);

        Task<INavigationResult> RefreshAsync();

        Task<INavigationResult> NavigateAsync(Uri uri, INavigationParameters parameter, NavigationTransitionInfo infoOverride);

        INavigationParameters CurrentParameters { get; }
    }
}
