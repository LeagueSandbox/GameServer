#region

using System;

#endregion

namespace ENet
{
    public unsafe class Peer
    {
        private Native.ENetPeer* _peer;

        public Peer(Native.ENetPeer* peer)
        {
            _peer = peer;
        }

        public bool IsSet
        {
            get { return _peer != null; }
        }

        public uint RoundTripTime
        {
            get { return _peer->lastRoundTripTime; }
        }

        public Native.ENetPeer* NativeData
        {
            get { return _peer; }
            set { _peer = value; }
        }

        public PeerState State
        {
            get { return IsSet ? _peer->state : PeerState.Uninitialized; }
        }

        public IntPtr UserData
        {
            get
            {
                CheckCreated();
                return _peer->data;
            }
            set
            {
                CheckCreated();
                _peer->data = value;
            }
        }

        public ushort Mtu
        {
            get { return _peer->mtu; }
            set { _peer->mtu = value; }
        }

        public Native.ENetAddress Address
        {
            get { return _peer->address; }
        }

        private void CheckCreated()
        {
            if (_peer == null)
            {
                throw new InvalidOperationException("No native peer.");
            }
        }

        public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
        {
            CheckCreated();
            Native.enet_peer_throttle_configure(_peer, interval, acceleration, deceleration);
        }

        public void Disconnect(uint data)
        {
            CheckCreated();
            Native.enet_peer_disconnect(_peer, data);
        }

        public void DisconnectLater(uint data)
        {
            CheckCreated();
            Native.enet_peer_disconnect_later(_peer, data);
        }

        public void DisconnectNow(uint data)
        {
            CheckCreated();
            Native.enet_peer_disconnect_now(_peer, data);
        }

        public void Ping()
        {
            CheckCreated();
            Native.enet_peer_ping(_peer);
        }

        public void Reset()
        {
            CheckCreated();
            Native.enet_peer_reset(_peer);
        }

        public bool Receive(out byte channelID, out Packet packet)
        {
            CheckCreated();
            Native.ENetPacket* nativePacket;
            nativePacket = Native.enet_peer_receive(_peer, out channelID);
            if (nativePacket == null)
            {
                packet = new Packet();
                return false;
            }
            packet = new Packet(nativePacket);
            return true;
        }

        public bool Send(byte channelID, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            return Send(channelID, data, 0, data.Length);
        }

        public bool Send(byte channelID, byte[] data, int offset, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            bool ret;
            using (var packet = new Packet())
            {
                packet.Create(data, offset, length);
                ret = Send(channelID, packet);
            }
            return ret;
        }

        public bool Send(byte channelID, Packet packet)
        {
            CheckCreated();
            packet.CheckCreated();
            return Native.enet_peer_send(_peer, channelID, packet.NativeData) >= 0;
        }
    }
}