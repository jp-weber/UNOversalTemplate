#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Navigation
{
    public interface IFrameFacade2
    {
        Frame Frame { get; }
    }
}
