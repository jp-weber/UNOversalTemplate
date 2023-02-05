namespace UNOversal.Navigation
{
    public static class NavigationParametersExtensions
    {
        public static void Remove(this INavigationParametersInternal nav, string key)
        {
            (nav as NavigationParameters).Remove(key);
        }
        internal static INavigationParametersInternal GetNavigationParametersInternal(this INavigationParameters parameters)
        {
            return (INavigationParametersInternal)parameters;
        }
    }
}
