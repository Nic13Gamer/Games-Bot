using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GamesBot
{
    public static class Bot
    {
        public static readonly bool DEBUG = true; // change in release
        public static readonly LogSeverity API_LOG_LEVEL = LogSeverity.Info; // change in release; default: Info
        public static readonly GatewayIntents GATEWAY_INTENTS = GatewayIntents.AllUnprivileged; // change in release; default: AllUnprivileged

        public static readonly string DATA_PATH = "C:/Users/nicho/Desktop/Games Bot/Data"; //Directory.GetCurrentDirectory() + "/Data";  THIS IS A OVERRIDE FOR DEVELOPMENT

        public static readonly SaveManager.BotOptions BotOptions = SaveManager.LoadBotOptions();

        public static DiscordSocketClient Client { get; private set; }
        public static InteractionService InteractionService { get; private set; }

        static bool ranReadyHandler = false;

        public static async Task Start()
        {
            Console.Title = "Games Bot";
            Terminal.Start();

            Client = new(new() { GatewayIntents = GATEWAY_INTENTS, LogLevel = API_LOG_LEVEL });
            InteractionService = new(Client);

            Client.Ready += async () =>
            {
                if (ranReadyHandler)
                    return;

                if (DEBUG)
                    await InteractionService.RegisterCommandsToGuildAsync(805241408544964669, true); // ursinhus luminosus
                else
                    await InteractionService.RegisterCommandsGloballyAsync(true);

                // start handlers that need the bot to be ready

                //SaveManager.LoadAll();

                InteractionHandler.Start();
                
                // ---

                Terminal.WriteLine("Bot started successfully!", Terminal.MessageType.INFO);

                await Client.SetGameAsync("Cool games :)");

                ranReadyHandler = true;
            };

            Client.Log += (log) =>
            {
                Terminal.WriteLine(log, Terminal.MessageType.API);
                return null;
            };

            await Client.LoginAsync(TokenType.Bot, BotOptions.token);
            await Client.StartAsync();

            // start all handlers

            await CommandHandler.Start();

            // ---

            await Task.Delay(Timeout.Infinite);
        }
    }
}
