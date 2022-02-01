using Newtonsoft.Json;
using System;
using System.IO;

namespace GamesBot
{
    public static class SaveManager
    {
        public static BotOptions LoadBotOptions()
        {
            string path = Bot.DATA_PATH + "/config.json";
            string json = File.ReadAllText(path).Trim();

            BotOptions options = JsonConvert.DeserializeObject<BotOptions>(json);
            return options;
        }

        public class BotOptions
        {
            public string token;

            public ulong botOwnerId;
        }
    }
}
