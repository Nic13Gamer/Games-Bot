using GamesBot.Utils;

using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

            pendingButtonCallback.callback?.Invoke(arg.User, pendingButtonCallback.param);

            pendingButtonCallbacks.Remove(pendingButtonCallback);

            return null;
        }

        public static ComponentBuilder WithCallbackButton(this ComponentBuilder builder, Action<IUser, object> callback, string label = null, ButtonStyle style = ButtonStyle.Primary, IEmote emote = null, string url = null, bool disabled = false, int row = 0, object callbackParam = null)
        {
            string customId = "btn-" + StringUtils.Random(18);

            builder.WithButton(label, customId, style, emote, url, disabled, row);
            pendingButtonCallbacks.Add(new PendingButtonCallback(callback, customId, callbackParam));

            return builder;
        }

        class PendingButtonCallback
        {
            public PendingButtonCallback(Action<IUser, object> callback, string customId, object param)
            {
                this.callback = callback;
                this.customId = customId;
                this.param = param;
            }

            public Action<IUser, object> callback;
            public string customId;
            public object param;
        }
    }
}
