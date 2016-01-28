#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace ENet
{
    public static unsafe partial class Native
    {
        #region Nested type: ENetAddress

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetAddress
        {
            public uint host;
            public ushort port;
        }

        #endregion

        #region Nested type: ENetCallbacks

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetCallbacks
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr malloc_cb(IntPtr size);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void free_cb(IntPtr memory);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void no_memory_cb();

            public IntPtr malloc, free, no_memory;
        }

        #endregion

        #region Nested type: ENetChannel

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetChannel
        {
            public ushort outgoingReliableSequenceNumber;
            public ushort outgoingUnreliableSequenceNumber;
            public ushort usedReliableWindows;
            public ushort reliableWindows;
            public ushort incomingReliableSequenceNumber;
            public ushort incomingUnreliableSequenceNumber;
            public ENetList* incomingReliableCommands;
            public ENetList* incomingUnreliableCommands;
        }

        #endregion

        #region Nested type: ENetCompressor

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetCompressor
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void compress_cb(IntPtr context, IntPtr inBuffers, IntPtr inBufferCount, IntPtr inLimit, IntPtr outData, IntPtr outLimit);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void decompress_cb(IntPtr context, IntPtr inData, IntPtr inLimit, IntPtr outData, IntPtr outLimit);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void destroy_cb(IntPtr context);

            public IntPtr context;
            public IntPtr compress, decompress, destroy;
        }

        #endregion

        #region Nested type: ENetEvent

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetEvent
        {
            public EventType type;
            public ENetPeer* peer;
            public byte channelID;
            public uint data;
            public ENetPacket* packet;
        }

        #endregion

        #region Nested type: ENetHost

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetHost
        {
           /* ENetSocket socket;
            ENetAddress address;                     //< Internet address of the host 
            uint incomingBandwidth;           //< downstream bandwidth of the host 
            uint outgoingBandwidth;           //< upstream bandwidth of the host 
            uint bandwidthThrottleEpoch;
            uint mtu;
            int recalculateBandwidthLimits;
            ENetPeer* peers;                       //< array of peers allocated for this host 
            IntPtr peerCount;                   //< number of peers allocated for this host 
            IntPtr channelLimit;                //< maximum number of channels allowed for connected peers 
            uint serviceTime;
            ENetList dispatchQueue;
            int continueSending;
            IntPtr packetSize;
            byte headerFlags;
            ENetProtocol commands[ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS];
            IntPtr commandCount;
            ENetBuffer buffers[ENET_BUFFER_MAXIMUM];
            IntPtr bufferCount;
            ENetChecksumCallback checksum;
            ENetAddress receivedAddress;
            byte receivedData[ENET_PROTOCOL_MAXIMUM_MTU];
            IntPtr receivedDataLength;
            uint totalSentData;               //< total data sent, user should reset to 0 as needed to prevent overflow 
            uint totalSentPackets;            //< total UDP packets sent, user should reset to 0 as needed to prevent overflow 
            uint totalReceivedData;           //< total data received, user should reset to 0 as needed to prevent overflow 
            uint totalReceivedPackets;        //< total UDP packets received, user should reset to 0 as needed to prevent overflow */
        }

        #endregion

        #region Nested type: ENetList

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetList
        {
            public ENetListNode* sentinel;
        }

        #endregion

        #region Nested type: ENetListNode

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetListNode
        {
            public ENetListNode* next, previous;
        }

        #endregion

        #region Nested type: ENetPacket

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetPacket
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void freeCallback_cb(IntPtr packet);

            public IntPtr referenceCount;
            public PacketFlags flags;
            public IntPtr data;
            public IntPtr dataLength;
            public IntPtr freeCallback;
        }

        #endregion

        #region Nested type: ENetPeer

        [StructLayout(LayoutKind.Sequential)]
        public struct ENetPeer
        {
            public ENetListNode dispatchList;
            public ENetHost* host;
            //public ushort outgoingPeerID;
            //public ushort incomingPeerID;
            public byte outgoingPeerID;
            public byte incomingPeerID;
            public uint connectID;
           // public byte outgoingSessionID;
           // public byte incomingSessionID;
            public ENetAddress address;
            public IntPtr data;
            public PeerState state;
            public ENetChannel* channels;
            public IntPtr channelcount;
            public uint incomingBandwidth;
            public uint outgoingBandwidth;
            public uint incomingBandwidthThrottleEpoch;
            public uint outgoingBandwidthThrottleEpoch;
            public uint incomingDataTotal;
            public uint outgoingDataTotal;
            public uint lastSendTime;
            public uint lastReceiveTime;
            public uint nextTimeout;
            public uint earliestTimeout;
            public uint packetLossEpoch;
            public uint packetsSent;
            public uint packetsLost;
            public uint packetLoss;
            public uint packetLossVariance;
            public uint packetThrottle;
            public uint packetThrottleLimit;
            public uint packetThrottleCounter;
            public uint packetThrottleEpoch;
            public uint packetThrottleAcceleration;
            public uint packetThrottleDeceleration;
            public uint packetThrottleInterval;
            //public uint pingInterval;
            //public uint timeoutLimit;
            //public uint timeoutMinimum;
           // public uint timeoutMaximum;
            public uint lastRoundTripTime;
            public uint lowestRoundTripTime;
            public uint lastRoundTripTimeVariance;
            public uint highestRoundTripTimeVariance;
            public uint roundTripTime;
            public uint roundTripTimeVariance;
            public ushort mtu;
            public uint windowSize;
            public uint reliableDataInTransit;
            public ushort outgoingReliableSequenceNumber;
            public ENetList* acknowledgements;
            public ENetList* sentReliableCommands;
            public ENetList* sentUnreliableCommands;
            public ENetList* outgoingReliableCommands;
            public ENetList* outgoingUnreliableCommands;
            public ENetList* dispatchedCommands;
            public int needsDispatch;
            public ushort incomingUnsequencedGroup;
            public ushort outgoingUnsequencedGroup;
            public uint unsequencedWindow;
            public uint eventData;
        }

        #endregion
    }
}