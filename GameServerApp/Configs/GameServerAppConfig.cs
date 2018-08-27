using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServerApp.Logic
{
    public class GameServerAppConfig
    {
        public string ClientLocation { get; private set; } = "C:\\LeagueSandbox\\League_Sandbox_Client";
        public bool AutoStartClient { get; private set; } = true;
        public bool AutoStartServer { get; private set; } = true;

        private GameServerAppConfig()
        {
        }

        public static GameServerAppConfig Default()
        {
            return new GameServerAppConfig();
        }

        public static GameServerAppConfig LoadFromJson(string json)
        {
            var result = new GameServerAppConfig();
            result.LoadConfig(json);
            return result;
        }

        public static GameServerAppConfig LoadFromFile(string path)
        {
            var result = new GameServerAppConfig();
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
            AutoStartServer = (bool)data.SelectToken("autoStartServer");
            ClientLocation = (string)data.SelectToken("clientLocation");
        }

        private void SaveConfig(string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, 
                    new {
                        clientLocation = ClientLocation,
                        autoStartClient = AutoStartClient,
                        autoStartServer = AutoStartServer
                    }
                );
            }
        }
    }
}