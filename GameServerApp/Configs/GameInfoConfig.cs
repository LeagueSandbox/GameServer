using System.IO;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServerApp.Logic
{
    public class GameInfoConfig
    {
        public string ClientLocation { get; private set; } = "C:\\LeagueSandbox\\League_Sandbox_Client";
        public bool AutoStartClient { get; private set; } = true;

        private GameInfoConfig()
        {
        }

        public static GameInfoConfig Default()
        {
            return new GameInfoConfig();
        }

        public static GameInfoConfig LoadFromJson(string json)
        {
            var result = new GameInfoConfig();
            result.LoadConfig(json);
            return result;
        }

        public static GameInfoConfig LoadFromFile(string path)
        {
            var result = new GameInfoConfig();
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
