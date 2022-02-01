using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace GamesBot
{
    public static class CommandHandler
    {
        static readonly DiscordSocketClient client = Bot.Client;
        static readonly InteractionService interactionService = Bot.InteractionService;

        public static async Task Start()
        {
            await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            client.InteractionCreated += HandleCommandInteraction;

            interactionService.SlashCommandExecuted += SlashCommandExecuted;
        }

        static async Task SlashCommandExecuted(SlashCommandInfo command, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess && result.Error != InteractionCommandError.UnknownCommand)
            {
                string reply = $"Alguma coisa deu errado! Motivo: " + result.ErrorReason;

                Terminal.WriteLine($"Bot use error [{result.ErrorReason}] by {context.User} ({context.User.Id})", Terminal.MessageType.WARN);
                
                await context.Interaction.RespondAsync(reply, ephemeral: true);
                return;
            }
            
            //SaveManager.SaveAll();
        }

        static Task HandleCommandInteraction(SocketInteraction arg)
        {
            var context = new SocketInteractionContext(client, arg);
            if (context.Interaction is not ISlashCommandInteraction) return null;

            var command = interactionService.SearchSlashCommand(context.Interaction as ISlashCommandInteraction);

            if (context.User.IsBot || context.Guild == null) return null;

            command.Command.ExecuteAsync(context, null);

            return null;
        }
    }
}
