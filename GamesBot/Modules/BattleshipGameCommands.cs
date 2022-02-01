using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace GamesBot.Modules
{
    [Group("battleship", "Play the battleship game")]
    public class BattleshipGameCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("play", "Invites another user to play battleship")]
        public async Task PlayCommand([Summary("user")] IGuildUser mention)
        {
            var components = new ComponentBuilder()
                .WithCallbackButton(() =>
                {
                    Terminal.WriteLine(mention.Nickname);
                }, "Accept");

            await mention.SendMessageAsync($"You have received a invite to play battleship from {Context.User.Mention}\nClick the button below to accept and start playing!",
                components: components.Build());

            await RespondAsync("A invite has been sent to the user's DM.", ephemeral: true);
        }
    }
}
