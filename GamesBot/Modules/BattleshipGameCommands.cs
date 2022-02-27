using Discord;
using Discord.Interactions;
using GamesBot.Games;
using System.Linq;
using System.Threading.Tasks;

namespace GamesBot
{
    [Group("battleship", "Play the battleship game")]
    public class BattleshipGameCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("play", "Invites another user to play battleship")]
        public async Task PlayCommand([Summary("user")] IGuildUser mention)
        {
            BattleshipGame game = GameManager.runningGames.Where(x => ((BattleshipGame)x).hostPlayer.user.Id == Context.User.Id ||
               ((BattleshipGame)x).otherPlayer.user.Id == Context.User.Id).FirstOrDefault() as BattleshipGame;
            if (game != default)
            {
                await RespondAsync("You are already playing a battleship game.", ephemeral: true);
                return;
            }

            var components = new ComponentBuilder()
                .WithCallbackButton(async (interaction) =>
                {
                    BattleshipGame game = GameManager.runningGames.Where(x => ((BattleshipGame)x).hostPlayer.user.Id == interaction.User.Id ||
                    ((BattleshipGame)x).otherPlayer.user.Id == interaction.User.Id).FirstOrDefault() as BattleshipGame;
                    if (game != default)
                    {
                        await interaction.RespondAsync("You are already playing a battleship game.", ephemeral: true);
                        return;
                    }

                    var hostUser = Context.User as IUser;
                    var otherUser = interaction.User as IUser;

                    if (otherUser.Id != mention.Id) return;

                    await interaction.RespondAsync($"{otherUser.Mention} battleship game with {hostUser.Mention} will begin soon!");

                    var newGame = new BattleshipGame(hostUser, otherUser, Context.Interaction, interaction);

                }, "Accept", ButtonStyle.Success);

            await RespondAsync($"{mention.Mention} has received an invite to play battleship with {Context.User.Mention}\nClick the button below to accept and start playing!",
                components: components.Build());
        }

        [SlashCommand("place", "Place a ship")]
        public async Task PlaceCommand(string position, BattleshipGame.ShipOrientation orientation)
        {
            BattleshipGame game = GameManager.runningGames.Where(x => ((BattleshipGame)x).hostPlayer.user.Id == Context.User.Id ||
            ((BattleshipGame)x).otherPlayer.user.Id == Context.User.Id).FirstOrDefault() as BattleshipGame;
            if(game == default)
            {
                await RespondAsync("You are not currently playing a battleship game.", ephemeral: true);
                return;
            }
            if(game.gamePhase != BattleshipGame.GamePhase.Placing)
            {
                await RespondAsync("Game is not in placing ships phase.", ephemeral: true);
                return;
            }

            position = position.Trim();

            Vector2 pos = new();
            try
            {
                pos.x = position.Length == 3 ? 10 : int.Parse(position[1].ToString());
                pos.y = char.ToUpper(position[0]) - 64;
            }
            catch (System.Exception e)
            {
                await RespondAsync(e.Message, ephemeral: true);
            }

            game.PlacePlayerShip(Context.Interaction, pos, orientation);
        }

        [SlashCommand("attack", "Attack your opponent")]
        public async Task AttackCommand(string position)
        {
            BattleshipGame game = GameManager.runningGames.Where(x => ((BattleshipGame)x).hostPlayer.user.Id == Context.User.Id ||
            ((BattleshipGame)x).otherPlayer.user.Id == Context.User.Id).FirstOrDefault() as BattleshipGame;
            if (game == default)
            {
                await RespondAsync("You are not currently playing a battleship game.", ephemeral: true);
                return;
            }
            if (game.gamePhase != BattleshipGame.GamePhase.Attacking)
            {
                await RespondAsync("Game is not in attacking ships phase.", ephemeral: true);
                return;
            }

            position = position.Trim();

            Vector2 pos = new();
            try
            {
                pos.x = position.Length == 3 ? 10 : int.Parse(position[1].ToString());
                pos.y = char.ToUpper(position[0]) - 64;
            }
            catch (System.Exception e)
            {
                await RespondAsync(e.Message, ephemeral: true);
            }

            game.AttackOpponentShip(Context.Interaction, pos);
        }

        // only ment to debug
        [SlashCommand("auto-finish", "debug command")]
        public async Task AutoFinishCommand()
        {
            BattleshipGame game = GameManager.runningGames.Where(x => ((BattleshipGame)x).hostPlayer.user.Id == Context.User.Id ||
            ((BattleshipGame)x).otherPlayer.user.Id == Context.User.Id).FirstOrDefault() as BattleshipGame;
            if (game == default)
            {
                await RespondAsync("You are not currently playing a battleship game.", ephemeral: true);
                return;
            }
            if (game.gamePhase != BattleshipGame.GamePhase.Placing)
            {
                await RespondAsync("Game is not in placing ships phase.", ephemeral: true);
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = new(i + 1, 1);

                game.AutoFinishPlayerShips(Context.Interaction, pos, i == 4);
            }
        }
    }
}
