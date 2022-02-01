using Discord;
using Discord.WebSocket;
using GamesBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesBot
{
    public static class InteractionHandler
    {
        static readonly DiscordSocketClient client = Bot.Client;

        static readonly List<PendingButtonCallback> pendingButtonCallbacks = new();

        public static void Start()
        {
            client.ButtonExecuted += HandleButtonClick;
        }

        static Task HandleButtonClick(SocketMessageComponent arg)
        {
            var pendingButtonCallback = pendingButtonCallbacks.Where(x => x.customId == arg.Data.CustomId).FirstOrDefault();
            if (pendingButtonCallback == default) return null;

            pendingButtonCallback.callback?.Invoke(arg);

            pendingButtonCallbacks.Remove(pendingButtonCallback);

            return null;
        }

        public static ComponentBuilder WithCallbackButton(this ComponentBuilder builder, Action<SocketMessageComponent> callback, string label = null, ButtonStyle style = ButtonStyle.Primary, IEmote emote = null, string url = null, bool disabled = false, int row = 0)
        {
            string customId = "btn-" + StringUtils.RandomString(18);

            builder.WithButton(label, customId, style, emote, url, disabled, row);
            pendingButtonCallbacks.Add(new PendingButtonCallback(callback, customId));

            return builder;
        }

        class PendingButtonCallback
        {
            public PendingButtonCallback(Action<SocketMessageComponent> callback, string customId)
            {
                this.callback = callback;
                this.customId = customId;
            }

            public Action<SocketMessageComponent> callback;
            public string customId;
        }
    }
}
