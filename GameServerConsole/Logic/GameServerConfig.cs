using System.IO;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServerConsole.Logic
{
    public class GameServerConfig
    {
        public string ClientLocation { get; private set; } = "C:\\LeagueSandbox\\League_Sandbox_Client";
        public bool AutoStartClient { get; private set; } = true;

        private GameServerConfig()
        {
        }

        public static GameServerConfig Default()
        {
            return new GameServerConfig();
        }

        public static GameServerConfig LoadFromJson(string json)
        {
            var result = new GameServerConfig();
            result.LoadConfig(json);
            return result;
        }

        public static GameServerConfig LoadFromFile(string path)
        {
            var result = new GameServerConfig();
            if (File.Exists(path))
            {
                result.LoadConfig(File.ReadAllText(path));
            }
            return result;
        }

        private void LoadConfig(string json)
        {
            var data = JObject.Parse(json);
            AutoStartClient = (bool)data.SelectToken("autoStartClient");
            ClientLocation = (string)data.SelectToken("clientLocation");
        }
    }
}
