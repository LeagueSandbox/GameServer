using GameServerCore;
namespace LeagueSandbox.GameServer.Packets
{
    public class NetworkIdManager : INetworkIdManager
    {
        protected uint _dwStart = 0x40000000; //new netid
        protected object _lock = new object();

        public uint GetNewNetId()
        {
            lock (_lock)
            {
                _dwStart++;
                return _dwStart;
            }
        }
    }
}
