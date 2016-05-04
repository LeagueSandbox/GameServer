#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace ENet
{
    public unsafe struct Packet : IDisposable
    {
        private Native.ENetPacket* _packet;

        public Packet(Native.ENetPacket* packet)
        {
            _packet = packet;
        }

        public IntPtr Data
        {
            get
            {
                CheckCreated();
                return _packet->data;
            }
        }

        public int Length
        {
            get
            {
                CheckCreated();
                if (_packet->dataLength.ToPointer() > (void*)int.MaxValue)
                {
                    throw new ENetException(0, "Packet too long!");
                }
                return (int)_packet->dataLength;
            }
        }

        public Native.ENetPacket* NativeData
        {
            get { return _packet; }
            set { _packet = value; }
        }

        public bool IsSet
        {
            get { return _packet != null; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_packet != null)
            {
                if (_packet->referenceCount == IntPtr.Zero)
                {
                    Native.enet_packet_destroy(_packet);
                }
                _packet = null;
            }
        }

        #endregion

        internal void CheckCreated()
        {
            if (_packet == null)
            {
                throw new InvalidOperationException("No native packet.");
            }
        }

        public void Create(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            Create(data, 0, data.Length);
        }

        public void Create(byte[] data, int offset, int length)
        {
            Create(data, offset, length, PacketFlags.None);
        }

        public void Create(byte[] data, int offset, int length, PacketFlags flags)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (offset < 0 || length < 0 || length > data.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            var ptr = Marshal.AllocHGlobal(length);
            Marshal.Copy(data, offset, ptr, length);
            Create(ptr, length, flags);
            Marshal.FreeHGlobal(ptr);
        }

        public void Create(IntPtr data, int length, PacketFlags flags)
        {
            if (_packet != null)
            {
                throw new InvalidOperationException("Already created.");
            }

            _packet = Native.enet_packet_create(data, (IntPtr)length, flags);
            if (_packet == null)
            {
                throw new ENetException(0, "Packet creation call failed.");
            }
        }

        public void CopyTo(byte[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            CopyTo(array, 0, array.Length);
        }

        public void CopyTo(byte[] array, int offset, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0 || length < 0 || length > array.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            CheckCreated();
            if (length > Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (length > 0)
            {
                Marshal.Copy((IntPtr)((byte*)Data + offset), array, offset, length);
            }
        }

        public byte[] GetBytes()
        {
            CheckCreated();
            var array = new byte[Length];
            CopyTo(array);
            return array;
        }

        public void Resize(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            CheckCreated();
            var ret = Native.enet_packet_resize(_packet, (IntPtr)length);
            if (ret < 0)
            {
                throw new ENetException(ret, "Packet resize call failed.");
            }
        }
    }
}