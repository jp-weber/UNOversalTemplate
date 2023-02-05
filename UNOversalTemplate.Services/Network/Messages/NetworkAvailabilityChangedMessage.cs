namespace UNOversal.Services.Network
{
    public class NetworkAvailabilityChangedMessage
    {
        public NetworkAvailabilityChangedMessage(ConnectionTypes connectionType)
        {
            ConnectionType = connectionType;
        }

        public ConnectionTypes ConnectionType { get; set; }
    }
}
