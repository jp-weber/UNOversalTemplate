﻿using System;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.UI.Xaml.Media.Animation;
#else
using Microsoft.UI.Xaml.Media.Animation;
#endif

namespace UNOversal.Navigation
{
    /// <summary>
    /// Provides page based navigation for ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to the most recent entry in the back navigation history by popping the calling Page off the navigation stack.
        /// </summary>
        /// <returns>If <c>true</c> a go back operation was successful. If <c>false</c> the go back operation failed.</returns>
        Task<INavigationResult> GoBackAsync();
        /// <summary>
        /// Navigates to the most recent entry in the back navigation history by popping the calling Page off the navigation stack.
        /// </summary>
        /// <param name="parameters">The navigation parameters</param>
        /// <returns>If <c>true</c> a go back operation was successful. If <c>false</c> the go back operation failed.</returns>
        Task<INavigationResult> GoBackAsync(INavigationParameters parameters);
        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The Uri to navigate to</param>
        /// <remarks>Navigation parameters can be provided in the Uri and by using the <paramref name="parameters"/>.</remarks>
        /// <example>
        /// Navigate(new Uri("MainPage?id=3&name=brian", UriKind.RelativeSource), parameters);
        /// </example>
        Task<INavigationResult> NavigateAsync(Uri uri);
        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The Uri to navigate to</param>
        /// <param name="parameters">The navigation parameters</param>
        /// <remarks>Navigation parameters can be provided in the Uri and by using the <paramref name="parameters"/>.</remarks>
        /// <example>
        /// Navigate(new Uri("MainPage?id=3&name=brian", UriKind.RelativeSource), parameters);
        /// </example>
        Task<INavigationResult> NavigateAsync(Uri uri, INavigationParameters parameters);
        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the target to navigate to.</param>
        Task<INavigationResult> NavigateAsync(string name);
        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the target to navigate to.</param>
        /// <param name="parameters">The navigation parameters</param>
        Task<INavigationResult> NavigateAsync(string name, INavigationParameters parameters);

        Task RefreshAsync();

        bool CanGoBack();
        event EventHandler CanGoBackChanged;
        Task<INavigationResult> GoBackAsync(INavigationParameters parameters, NavigationTransitionInfo infoOverride);

        bool CanGoForward();
        event EventHandler CanGoForwardChanged;
        Task<INavigationResult> GoForwardAsync();
        Task<INavigationResult> GoForwardAsync(INavigationParameters parameter);

        Task<INavigationResult> NavigateAsync(string path, INavigationParameters parameter, NavigationTransitionInfo infoOverride);
        Task<INavigationResult> NavigateAsync(Uri path, INavigationParameters parameter, NavigationTransitionInfo infoOverride);
    }
}
