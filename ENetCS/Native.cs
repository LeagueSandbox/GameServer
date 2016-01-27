#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace ENet
{
    public static unsafe partial class Native
    {
        private const string LIB = "libenet.dll";

        public const int ENET_PEER_PACKET_THROTTLE_SCALE = 32;
        public const int ENET_PEER_PACKET_THROTTLE_ACCELERATION = 2;
        public const int ENET_PEER_PACKET_THROTTLE_DECELERATION = 2;
        public const int ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;
        public const int ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT = 0x01;
        public const int ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT = 0xff;
        public const int ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xfff;
        public const uint ENET_VERSION = (1 << 16) | (3 << 8) | (1);
        public const uint ENET_HOST_ANY = 0;
        public const uint ENET_HOST_BROADCAST = 0xffffffff;

        #region Address Functions

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_set_host(ref ENetAddress address, byte* hostName);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_set_host(ref ENetAddress address, byte[] hostName);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_get_host(ref ENetAddress address, byte* hostName, IntPtr nameLength);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_get_host(ref ENetAddress address, byte[] hostName, IntPtr nameLength);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_get_host_ip(ref ENetAddress address, byte* hostIP, IntPtr ipLength);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_get_host_ip(ref ENetAddress address, byte[] hostIP, IntPtr ipLength);

        #endregion

        #region Global Functions

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_deinitialize();

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_initialize();

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_initialize_with_callbacks(uint version, ref ENetCallbacks inits);

        #endregion

        #region Host Functions

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_host_compress_with_range_coder(ENetHost* host);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern ENetHost* enet_host_create(ENetAddress* address,
                                                        IntPtr peerLimit, IntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern ENetHost* enet_host_create(ref ENetAddress address,
                                                        IntPtr peerLimit, IntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_host_destroy(ENetHost* host);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern ENetPeer* enet_host_connect(ENetHost* host, ref ENetAddress address, IntPtr channelCount, uint data);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_host_broadcast(ENetHost* host, byte channelID, ENetPacket* packet);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_host_compress(ENetHost* host, ENetCompressor* compressor);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_host_channel_limit(ENetHost* host, IntPtr channelLimit);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_host_bandwidth_limit(ENetHost* host, uint incomingBandwidth, uint outgoingBandwidth);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_host_flush(ENetHost* host);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_host_check_events(ENetHost* host, out ENetEvent @event);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_host_service(ENetHost* host, ENetEvent* @event, uint timeout);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_host_service(ENetHost* host, out ENetEvent @event, uint timeout);

        #endregion

        #region Miscellaneous Functions

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint enet_time_get();

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_time_set(uint newTimeBase);

        #endregion

        #region Packet Functions

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern ENetPacket* enet_packet_create(IntPtr data, IntPtr dataLength, PacketFlags flags);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_packet_destroy(ENetPacket* packet);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_packet_resize(ENetPacket* packet, IntPtr dataLength);

        #endregion

        #region Peer Functions

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_peer_throttle_configure(ENetPeer* peer, uint interval, uint acceleration, uint deceleration);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_peer_send(ENetPeer* peer, byte channelID, ENetPacket* packet);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern ENetPacket* enet_peer_receive(ENetPeer* peer, out byte channelID);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_peer_reset(ENetPeer* peer);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_peer_ping(ENetPeer* peer);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_peer_disconnect_now(ENetPeer* peer, uint data);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_peer_disconnect(ENetPeer* peer, uint data);

        [DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_peer_disconnect_later(ENetPeer* peer, uint data);

        #endregion

        #region C# Utility

        public static bool memcmp(byte[] s1, byte[] s2)
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentNullException();
            }
            if (s1.Length != s2.Length)
            {
                return false;
            }

            for (var i = 0; i < s1.Length; i++)
            {
                if (s1[i] != s2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int strlen(byte[] s)
        {
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            int i;
            for (i = 0; i < s.Length && s[i] != 0; i++) ;
            return i;
        }

        #endregion
    }
}