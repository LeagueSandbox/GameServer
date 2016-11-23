namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class NetworkIdManager
    {
        protected uint _dwStart = 0x40000000; //new netid
        protected object _lock = new object();

        public uint GetNewNetID()
        {
            lock (_lock)
            {
                _dwStart++;
                return _dwStart;
            }
        }
    }
}
