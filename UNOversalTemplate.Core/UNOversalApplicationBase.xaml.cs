using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using LaunchActivatedEventArgs = Windows.ApplicationModel.Activation.LaunchActivatedEventArgs;
#else
using Microsoft.UI.Xaml;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
#endif

namespace UNOversal
{
    public abstract partial class UNOversalApplicationBase : Application
    {
        protected override sealed async void OnActivated(IActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Activate));
#if WINDOWS_UWP
        protected override sealed async void OnCachedFileUpdaterActivated(CachedFileUpdaterActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Activate));
        protected override sealed async void OnFileActivated(FileActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Activate));
        protected override sealed async void OnFileOpenPickerActivated(FileOpenPickerActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Activate));
        protected override sealed async void OnFileSavePickerActivated(FileSavePickerActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Activate));
        protected override sealed async void OnSearchActivated(SearchActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Activate));
        protected override sealed async void OnShareTargetActivated(ShareTargetActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Activate));
        protected override sealed async void OnBackgroundActivated(BackgroundActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Background));
#endif
        protected override sealed async void OnLaunched(LaunchActivatedEventArgs e) => await InternalStartAsync(new ApplicationArgs(e, StartKinds.Launch));

#if WINDOWS_UWP
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            base.OnWindowCreated(args);
            //WindowService.ForwardWindowCreated(args);
        }
#endif
    }
}
