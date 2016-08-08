using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Logic
{
    public class Settings
    {
        private JToken _settings;

        private Settings(JToken settings)
        {
            _settings = settings;
        }

        public string RadsPath { get { return (string)_settings.SelectToken("radsPath"); } }

        public static Settings Load(string path)
        {
            var settings = new Dictionary<string, string>();
            
            var json = File.ReadAllText(path);
            var data = JObject.Parse(json);

            return new Settings(data);
        }
    }
}
