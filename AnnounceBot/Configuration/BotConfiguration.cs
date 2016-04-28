using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceBot.Configuration
{
    class BotConfiguration
    {
        private const string path = "configuration.json";
        private static BotConfiguration _instance = new BotConfiguration();

        // Load the configuration.
        public static BotConfiguration Load()
        {
            // If no file exists, we create a new configuration file.
            if (!File.Exists(path))
            {
                Console.WriteLine("No configuration file was found, please modify the newly created configuration.json and restart the program.");
                Save();
            }

            // Deserialize the configuration object.
            _instance = JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText(path));

            return _instance;

        }

        // Save the configuration.
        public static void Save()
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(_instance, Formatting.Indented));
        }


        // Discord
        public class DiscordSettings
        {
            [JsonProperty("token")]
            public string Token = "example";
        }


        [JsonProperty("discord")]
        private DiscordSettings _discord = new DiscordSettings();
        public static DiscordSettings Discord => _instance._discord;

        // Black Desert
        public class BDOSettings
        {
            [JsonProperty("channels")]
            public List<string> ChannelNames = new List<string>();

            [JsonProperty("patch_url")]
            public string PatchUrl = "http://akamai-gamecdn.blackdesertonline.com/live001/game/config/config.patch.version";

            [JsonProperty("update_time")]
            public int UpdateTime = 60;

            [JsonProperty("cache_file")]
            public string CacheFile = "patch_ver.cache";

            [JsonProperty("notify_new_version_on_restart")]
            public bool Notify = true;
        }


        [JsonProperty("bdo")]
        private BDOSettings _bdo = new BDOSettings();
        public static BDOSettings BDO => _instance._bdo;

    }
}
