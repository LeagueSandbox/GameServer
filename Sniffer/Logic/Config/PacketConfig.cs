using SharpConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnifferApp.Logic.Config
{
    class PacketConfig
    {
        private Dictionary<byte, string> C2SPackets = new Dictionary<byte, string>();
        private Dictionary<byte, string> S2CPackets = new Dictionary<byte, string>();
        private static PacketConfig _instance;

        public PacketConfig()
        {
            var config = Configuration.LoadFromFile("Packets.ini");
            foreach (var section in config)
            {
                foreach (var setting in section)
                {
                    switch (section.Name)
                    {
                        case "S2C":
                            S2CPackets.Add(byte.Parse(setting.Name, System.Globalization.NumberStyles.HexNumber), setting.StringValue);
                            break;
                        case "C2S":
                            C2SPackets.Add(byte.Parse(setting.Name, System.Globalization.NumberStyles.HexNumber), setting.StringValue);
                            break;
                    }
                }
            }
        }

        public List<KeyValuePair<string, string>> getConfig(byte packetId, bool s2c)
        {
            if (s2c)
            {
                if (S2CPackets.ContainsKey(packetId))
                    return parse(C2SPackets[packetId]);
            }
            else
            {
                if (C2SPackets.ContainsKey(packetId))
                    return parse(S2CPackets[packetId]);
            }

            return new List<KeyValuePair<string, string>>();
        }

        private List<KeyValuePair<string, string>> parse(string configLine)
        {
            var ret = new List<KeyValuePair<string, string>>();
            foreach (var config in configLine.Split(')'))
            {
                var kk = config.Split('(');
                if (kk.Length < 2)
                    continue;
                var type = kk[0];
                var name = kk[1];
                ret.Add(new KeyValuePair<string, string>(type.ToLower(), name));
            }
            return ret;
        }

        public static PacketConfig getInstance()
        {
            if (_instance == null)
                _instance = new PacketConfig();

            return _instance;
        }
    }
}
