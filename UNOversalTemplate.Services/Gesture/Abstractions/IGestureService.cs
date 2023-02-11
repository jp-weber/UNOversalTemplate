using System;
using System.Collections.Generic;

namespace Project2FA.Services
{
    public interface IGestureService
    {
        GestureBlocker CreateBlocker(Gesture gesture, BlockerPeriod period);

        Dictionary<string, Action<KeyDownEventArgs>> KeyDownCallbacks { get; }
        Dictionary<string, Action> BackRequestedCallbacks { get; }
        Dictionary<string, Action> ForwardRequestedCallbacks { get; }
        Dictionary<string, Action> MenuRequestedCallbacks { get; }
        Dictionary<string, Action> RefreshRequestedCallbacks { get; }
        Dictionary<string, Action> SearchRequestedCallbacks { get; }

        void RaiseBackRequested();
        void RaiseForwardRequested();
        void RaiseMenuRequested();
        void RaiseRefreshRequested();
        void RaiseSearchRequested();
    }
}