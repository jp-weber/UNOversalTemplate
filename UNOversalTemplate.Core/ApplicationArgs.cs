using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Core.Preview;

#if WINDOWS_UWP
using LaunchActivatedEventArgs = Windows.ApplicationModel.Activation.LaunchActivatedEventArgs;
#else
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;
#endif

namespace UNOversal
{
    public enum StopKind
    {
        Suspending,
        CloseRequested,
        CoreWindowClosed,
        CoreApplicationExiting
    }

    public interface IStopArgs
    {
        StopKind StopKind { get; }
        SuspendingEventArgs SuspendingEventArgs { get; }
        SystemNavigationCloseRequestedPreviewEventArgs CloseRequestedPreviewEventArgs { get; }
        CoreWindowEventArgs CoreWindowEventArgs { get; }
        object CoreApplicationEventArgs { get; }
    }

    public class StopArgs : IStopArgs
    {
        public StopArgs(StopKind kind)
        {
            StopKind = kind;
        }
        public StopKind StopKind { get; }
        public SuspendingEventArgs SuspendingEventArgs { get; internal set; }
        public SystemNavigationCloseRequestedPreviewEventArgs CloseRequestedPreviewEventArgs { get; internal set; }
        public CoreWindowEventArgs CoreWindowEventArgs { get; internal set; }
        public object CoreApplicationEventArgs { get; internal set; }
    }

    public class ApplicationArgs : IApplicationArgs
    {
        public ApplicationArgs(IActivatedEventArgs args, StartKinds startKind)
        {
            Arguments = args;
            StartKind = startKind;
        }

        public ApplicationArgs(LaunchActivatedEventArgs args, StartKinds startKind)
        {
            Arguments = args;
            StartKind = startKind;
        }

        public ApplicationArgs(BackgroundActivatedEventArgs args, StartKinds startKind)
        {
            Arguments = args;
            StartKind = startKind;
        }

        public override string ToString()
        {
            return $"Args:{Arguments?.GetType()} Kind:{StartKind} Cause:{StartCause}";
        }

        public object Arguments { get; internal set; }

        public StartKinds StartKind { get; internal set; }

        public StartCauses StartCause
        {
            get
            {
                switch (Arguments)
                {
                    case IToastNotificationActivatedEventArgs t: return StartCauses.Toast;
#if WINDOWS_UWP
                    case ILaunchActivatedEventArgs p when (p?.TileId == "App" && string.IsNullOrEmpty(p?.Arguments)): return StartCauses.PrimaryTile;
                    case ILaunchActivatedEventArgs j when (j?.TileId == "App" && !string.IsNullOrEmpty(j?.Arguments)): return StartCauses.JumpListItem;
                    case ILaunchActivatedEventArgs s when (!string.IsNullOrEmpty(s?.TileId) && s?.TileId != "App"): return StartCauses.SecondaryTile;
#endif
                    case IBackgroundActivatedEventArgs b: return StartCauses.BackgroundTrigger;
                    case IFileActivatedEventArgs f: return StartCauses.File;
                    case IPrelaunchActivatedEventArgs p: return StartCauses.Prelaunch;
                    case IProtocolActivatedEventArgs p: return StartCauses.Protocol;
                    case ILockScreenActivatedEventArgs l: return StartCauses.LockScreen;
                    case IShareTargetActivatedEventArgs s: return StartCauses.ShareTarget;
                    case IVoiceCommandActivatedEventArgs v: return StartCauses.VoiceCommand;
                    case ISearchActivatedEventArgs s: return StartCauses.Search;
                    case IDeviceActivatedEventArgs d: return StartCauses.Device;
                    case IDevicePairingActivatedEventArgs d: return StartCauses.DevicePairing;
                    case IContactPanelActivatedEventArgs c: return StartCauses.ContactPanel;
                    // https://blogs.windows.com/buildingapps/2017/07/28/restart-app-programmatically/#1sGJmiirzC2MtROE.97
                    case ICommandLineActivatedEventArgs c: return StartCauses.CommandLine;
                    case IActivatedEventArgs r when (r != null && r.Kind == ActivationKind.Launch && r.PreviousExecutionState == ApplicationExecutionState.Terminated): return StartCauses.Restart;
                    case null: return StartCauses.Undetermined;
                    default: return StartCauses.Undetermined;
                }
            }
        }
    }
}
