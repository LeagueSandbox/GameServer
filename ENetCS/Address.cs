#region

using System;
using System.Net;
using System.Text;

#endregion

namespace ENet
{
    public unsafe struct Address : IEquatable<Address>
    {
        public const uint IPv4HostAny = Native.ENET_HOST_ANY;
        public const uint IPv4HostBroadcast = Native.ENET_HOST_BROADCAST;

        private Native.ENetAddress _address;

        public Address(uint host, ushort port)
        {
            _address = new Native.ENetAddress();
            _address.host = host;
            _address.port = port;
        }

        public Native.ENetAddress NativeData
        {
            get { return _address; }
            set { _address = value; }
        }

        public uint IPv4Host
        {
            get { return _address.host; }
            set { _address.host = value; }
        }

        public ushort Port
        {
            get { return _address.port; }
            set { _address.port = value; }
        }

        public AddressType Type
        {
            get { return AddressType.IPv4; }
        }

        #region IEquatable<Address> Members

        public bool Equals(Address address)
        {
            return Type == address.Type && Port == address.Port
                   && Native.memcmp(GetHostBytes(), address.GetHostBytes());
        }

        #endregion

        public override bool Equals(object obj)
        {
            return obj is Address && Equals((Address)obj);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Port.GetHashCode() ^ IPv4Host.GetHashCode();
        }

        public byte[] GetHostBytes()
        {
            return BitConverter.GetBytes(IPAddress.NetworkToHostOrder((int)IPv4Host));
        }

        public string GetHostName()
        {
            var name = new byte[256];
            fixed (byte* hostName = name)
            {
                if (Native.enet_address_get_host(ref _address, hostName, (IntPtr)name.Length) < 0)
                {
                    return null;
                }
            }
            return BytesToString(name);
        }

        public string GetHostIP()
        {
            var ip = new byte[256];
            fixed (byte* hostIP = ip)
            {
                if (Native.enet_address_get_host_ip(ref _address, hostIP, (IntPtr)ip.Length) < 0)
                {
                    return null;
                }
            }
            return BytesToString(ip);
        }

        public bool SetHost(string hostName)
        {
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            return Native.enet_address_set_host(ref _address,
                                                Encoding.ASCII.GetBytes(hostName)) == 0;
        }

        private static string BytesToString(byte[] bytes)
        {
            try
            {
                return Encoding.ASCII.GetString(bytes, 0, Native.strlen(bytes));
            }
            catch
            {
                return null;
            }
        }
    }
}