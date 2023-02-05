using System;

namespace UNOversal.Services.Network
{
    public class AvailabilityChangedEventArgs : EventArgs
    {
        public ConnectionTypes ConnectionType { get; set; }
    }
}
