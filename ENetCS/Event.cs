namespace ENet
{
    public unsafe class Event
    {
        internal Native.ENetEvent _event;

        public Event(Native.ENetEvent @event)
        {
            _event = @event;
        }
        public Event()
        {

        }

        public byte ChannelID
        {
            get { return _event.channelID; }
        }

        public uint Data
        {
            get { return _event.data; }
            set { _event.data = value; }
        }

        public Native.ENetEvent NativeData
        {
            get { return _event; }
            set { _event = value; }
        }

        public Packet Packet
        {
            get { return new Packet(_event.packet); }
        }

        public Peer Peer
        {
            get { return new Peer(_event.peer); }
        }

        public EventType Type
        {
            get { return _event.type; }
        }
    }
}