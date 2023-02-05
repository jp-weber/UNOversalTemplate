﻿
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Frame = Windows.UI.Xaml.Controls.Frame;
#else
using Microsoft.UI.Xaml.Controls;
using Frame = Microsoft.UI.Xaml.Controls.Frame;
#endif
using System;


namespace UNOversal.Navigation
{
    public static class NavigationFactory
    {
        /// <summary>
        /// Creates a navigation service
        /// </summary>
        /// <returns>INavigationService</returns>
        public static INavigationService Create(string id = null)
        {
            return Create(new Frame(), id);
        }

        /// <summary>
        /// Creates a navigation service for pre-existing frame
        /// </summary>
        /// <param name="frame">Required XAML Frame</param>
        /// <returns>INavigationService</returns>
        public static INavigationService Create(Frame frame, string id = null)
        {
            frame = frame ?? new Frame();
            id = id ?? Guid.NewGuid().ToString();
            return new NavigationService(frame, id);
        }
    }
}
