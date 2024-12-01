using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOversal.Ioc;
using UNOversal.Logging;

#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Frame = Windows.UI.Xaml.Controls.Frame;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Frame = Microsoft.UI.Xaml.Controls.Frame;
#endif

namespace UNOversal.Navigation
{
    public sealed partial class NavigationService : INavigationService, INavigationService2
    {
        IFrameFacade INavigationService2.FrameFacade => _frame;

        public static Dictionary<Frame, INavigationService> Instances { get; } = new Dictionary<Frame, INavigationService>();

        private readonly IFrameFacade _frame;
        private readonly ILoggerFacade _logger;

        internal NavigationService(Frame frame, string id)
        {
            _frame = new FrameFacade(frame, this, id);
            _frame.CanGoBackChanged += (s, e) =>
                CanGoBackChanged?.Invoke(this, EventArgs.Empty);
            _frame.CanGoForwardChanged += (s, e) =>
                CanGoForwardChanged?.Invoke(this, EventArgs.Empty);
            Instances.Add(frame, this);
            _logger = UNOversalApplicationBase.Current.Container.Resolve<ILoggerFacade>();
        }

        public async Task RefreshAsync()
            => await _frame.RefreshAsync();

        #region GoForward

        public event EventHandler CanGoForwardChanged;

        public bool CanGoForward()
            => _frame.CanGoForward();

        public async Task<INavigationResult> GoForwardAsync()
            => await GoForwardAsync(
                parameters: default);

        public async Task<INavigationResult> GoForwardAsync(INavigationParameters parameters)
        {
            if (parameters == null && (_frame as IFrameFacade2).Frame.ForwardStack.Any())
            {
                var previous = (_frame as IFrameFacade2).Frame.ForwardStack.Last().Parameter?.ToString();
                parameters = new NavigationParameters(previous);
            }

            return await _frame.GoForwardAsync(
                  parameters: parameters);
        }
        #endregion

        #region GoBack

        public event EventHandler CanGoBackChanged;

        public bool CanGoBack()
            => _frame.CanGoBack();

        /// <summary>
        /// Navigates to the most recent entry in the back navigation history by popping the calling Page off the navigation stack.
        /// </summary>
        /// <returns>If <c>true</c> a go back operation was successful. If <c>false</c> the go back operation failed.</returns>
        public async Task<INavigationResult> GoBackAsync()
            => await GoBackAsync(
                parameters: default,
                infoOverride: default);

        /// <summary>
        /// Navigates to the most recent entry in the back navigation history by popping the calling Page off the navigation stack.
        /// </summary>
        /// <param name="parameters">The navigation parameters</param>
        /// <returns>If <c>true</c> a go back operation was successful. If <c>false</c> the go back operation failed.</returns>
        public async Task<INavigationResult> GoBackAsync(INavigationParameters parameters)
            => await GoBackAsync(
                parameters: parameters,
                infoOverride: default);

        public async Task<INavigationResult> GoBackAsync(INavigationParameters parameters = null, NavigationTransitionInfo infoOverride = null)
        {
            if (parameters == null && (_frame as IFrameFacade2).Frame.BackStack.Any())
            {
                var previous = (_frame as IFrameFacade2).Frame.BackStack.Last().Parameter?.ToString();
                if (previous is null)
                {
                    parameters = new NavigationParameters();
                }
                else
                {
                    parameters = new NavigationParameters(previous);
                }
            }

            return await _frame.GoBackAsync(
                    parameters: parameters,
                    infoOverride: infoOverride);
        }
        #endregion

        #region Navigate(string)

        public async Task<INavigationResult> NavigateAsync(string path)
            => await NavigateAsync(
                uri: new Uri(path, UriKind.RelativeOrAbsolute),
                parameters: default(INavigationParameters),
                infoOverride: default);

        public async Task<INavigationResult> NavigateAsync(string path, INavigationParameters parameters)
            => await NavigateAsync(
                uri: new Uri(path, UriKind.RelativeOrAbsolute),
                parameters: parameters,
                infoOverride: default);

        public async Task<INavigationResult> NavigateAsync(string path, params (string Key, object Value)[] parameters)
            => await NavigateAsync(
                uri: new Uri(path, UriKind.RelativeOrAbsolute),
                parameters: parameters,
                infoOverride: default);

        public async Task<INavigationResult> NavigateAsync(string path, INavigationParameters parameter, NavigationTransitionInfo infoOverride)
            => await NavigateAsync(
                uri: new Uri(path, UriKind.RelativeOrAbsolute),
                parameters: parameter,
                infoOverride: infoOverride);
        #endregion

        #region Navigate(Uri)

        public async Task<INavigationResult> NavigateAsync(Uri uri)
            => await NavigateAsync(
                uri: uri,
                parameters: default(INavigationParameters),
                infoOverride: default);

        public async Task<INavigationResult> NavigateAsync(Uri uri, INavigationParameters parameters)
            => await NavigateAsync(
                uri: uri,
                parameters: parameters,
                infoOverride: default);

        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The Uri to navigate to</param>
        /// <param name="parameters">The navigation parameters</param>
        /// <param name="infoOverride">The transition info</param>
        /// <returns><see cref="INavigationResult"/> indicating whether the request was successful or if there was an encountered <see cref="Exception"/>.</returns>
        /// <remarks>Navigation parameters can be provided in the Uri and by using the <paramref name="parameters"/>.</remarks>
        /// <example>
        /// NavigateAsync(new Uri("MainPage?id=3&amp;name=dan", UriKind.RelativeSource), ("person", person), ("foo", bar));
        /// </example>
        public async Task<INavigationResult> NavigateAsync(Uri uri, NavigationTransitionInfo infoOverride, params (string Key, object Value)[] parameters)
        {
            _logger.Log($"{nameof(NavigationService)}.{nameof(NavigateAsync)}(uri:{uri} parameter:{parameters} info:{infoOverride})", Category.Info, Priority.None);
            return await NavigateAsync(
                uri: uri,
                parameters: GetNavigationParameters(parameters),
                infoOverride: default);
        }

        public async Task<INavigationResult> NavigateAsync(Uri uri, INavigationParameters parameters, NavigationTransitionInfo infoOverride)
        {
            _logger.Log($"{nameof(NavigationService)}.{nameof(NavigateAsync)}(uri:{uri} parameter:{parameters} info:{infoOverride})", Category.Info, Priority.None);

            return await _frame.NavigateAsync(
                uri: uri,
                parameter: parameters,
                infoOverride: infoOverride);
        }
        #endregion

        private static INavigationParameters GetNavigationParameters((string Key, object Value)[] parameters)
        {
            var navParams = new NavigationParameters();
            foreach (var (Key, Value) in parameters)
            {
                navParams.Add(Key, Value);
            }
            return navParams;
        }
    }
}
