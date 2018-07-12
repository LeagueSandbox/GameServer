namespace LeagueSandbox.GameServer.Logic.Packets
{
    public static class NetworkIdManager
    {
        private static uint _dwStart = 0x40000000; //new netid
        private static object _lock = new object();

        public static uint GetNewNetId()
        {
            lock (_lock)
            {
                _dwStart++;
                return _dwStart;
            }
        }
    }
}
