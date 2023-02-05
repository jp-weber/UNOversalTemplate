
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace UNOversal.Navigation
{
    public static class INavigationServiceExtensions
    {
        public static Frame GetXamlFrame(this INavigationService service)
        {
            return (service.GetFrameFacade() as IFrameFacade2).Frame;
        }

        public static IFrameFacade GetFrameFacade(this INavigationService service)
        {
            return (service as INavigationService2).FrameFacade;
        }

        //public static INavigationService SetAsWindowContent(this INavigationService service, Window window = null)
        //{
        //    Frame frame = service.GetXamlFrame() ?? throw new NullReferenceException(nameof(Frame));
        //    if (window == null && !WindowService.TryGetWindow(frame, out window))
        //    {
        //        throw new NullReferenceException(nameof(window));
        //    }
        //    window.Content = frame;
        //    return service;
        //}

        //public static INavigationService ActivateWindow(this INavigationService service, Window window = null)
        //{
        //    window ??= Window.Current;
        //    window.Activate();
        //    while (!WindowService.AllWindows.Contains(window))
        //    {
        //        Task.Delay(50).RunSynchronously();
        //    }
        //    return service;
        //}

        //public static INavigationService AttachGestures(this INavigationService service, Window window, params Gesture[] gestures)
        //{
        //    INavigationService2 service2 = service as INavigationService2;
        //    IGestureService gesture_service = GestureService.GetForCurrentView(window.CoreWindow);
        //    foreach (Gesture gesture in gestures)
        //    {
        //        switch (gesture)
        //        {
        //            case Gesture.Back:
        //                AddOnlyOne(gesture_service.BackRequestedCallbacks, service2.FrameFacade.Id, async () => await service.GoBackAsync());
        //                break;
        //            case Gesture.Forward:
        //                AddOnlyOne(gesture_service.ForwardRequestedCallbacks, service2.FrameFacade.Id, async () => await service.GoForwardAsync());
        //                break;
        //            case Gesture.Refresh:
        //                AddOnlyOne(gesture_service.RefreshRequestedCallbacks, service2.FrameFacade.Id, async () => await service.RefreshAsync());
        //                break;
        //        }
        //    }
        //    return service;

        //    void AddOnlyOne(Dictionary<string, Action> dictionary, string id, Action action)
        //    {
        //        dictionary.Remove(id);
        //        dictionary.Add(id, action);
        //    }
        //}
    }
}
