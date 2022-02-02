using Discord;
using Discord.WebSocket;
using GamesBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GamesBot.Games
{
    public class BattleshipGame : Game
    {
        public readonly Player hostPlayer;
        public readonly Player otherPlayer;

        public bool turn = true; // true - host; false - other

        public GamePhase gamePhase = GamePhase.Placing;

        public BattleshipGame(IUser hostUser, IUser otherUser, SocketInteraction hostInteraction, SocketInteraction otherInteraction)
        {
            hostPlayer = new(hostUser, hostInteraction);
            otherPlayer = new(otherUser, otherInteraction);

            GameManager.runningGames.Add(this);

            SetupPlayerShips();
        }

        async void SetupPlayerShips()
        {
            Ship[] availableShips = CreateAvailableShips();

            hostPlayer.availableShips = availableShips.ToList();
            otherPlayer.availableShips = availableShips.ToList();

            var ship = hostPlayer.availableShips[0];
            var board = ImageUtils.GenerateBattleshipGameBoard(Array.Empty<Vector2>(), Array.Empty<Vector2>(), true, false);

            await hostPlayer.interaction.FollowupWithFileAsync(board,
                text: $"Please place your ship. The length is {ship.length}.\nEx.: /battleship place A3 Horizontal", ephemeral: true);
            await otherPlayer.interaction.FollowupWithFileAsync(board,
                text: $"Please place your ship. The length is {ship.length}.\nEx.: /battleship place A3 Horizontal", ephemeral: true);

            hostPlayer.currentPlacingShip = ship;
            otherPlayer.currentPlacingShip = ship;
        }

        public async void PlacePlayerShip(SocketInteraction interaction, Vector2 pos, ShipOrientation orientation)
        {
            if (gamePhase != GamePhase.Placing) return;

            Player player = interaction.User.Id == hostPlayer.user.Id ? hostPlayer : otherPlayer;

            if (player.availableShips == null || player.availableShips.Count == 0)
            {
                await interaction.RespondAsync("You already placed all your ships!", ephemeral: true);
                return;
            }

            if (orientation == ShipOrientation.Horizontal)
            {
                List<Vector2> blocks = new();

                for (int i = 0; i < player.currentPlacingShip.length; i++)
                {
                    Vector2 block = new(pos.x + i, pos.y);

                    if (block.x > 10 || block.y > 10 || HasShipInBlock(player, block))
                    {
                        await interaction.RespondAsync("This position is not valid!", ephemeral: true);
                        return;
                    }

                    blocks.Add(block);
                }

                player.currentPlacingShip.blocks = blocks.ToArray();
            }
            else
            {
                List<Vector2> blocks = new();

                for (int i = 0; i < player.currentPlacingShip.length; i++)
                {
                    Vector2 block = new(pos.x, pos.y + i);

                    if (block.x > 10 || block.y > 10 || HasShipInBlock(player, block))
                    {
                        await interaction.RespondAsync("This position is not valid!", ephemeral: true);
                        return;
                    }

                    blocks.Add(block);
                }

                player.currentPlacingShip.blocks = blocks.ToArray();
            }

            player.ships.Add(player.availableShips[0]);
            player.availableShips.RemoveAt(0);
            player.currentPlacingShip = player.availableShips.Count != 0 ? player.availableShips[0] : null;

            Vector2[] shipBlocks = Array.Empty<Vector2>();
            foreach (var ship in player.ships)
                shipBlocks = shipBlocks.Concat(ship.blocks).ToArray();

            string reply = player.currentPlacingShip == null ? "You have finished placing your ships, wait for your opponent."
                : $"Please place your ship. The length is { player.currentPlacingShip.length }.\nEx.: / battleship place A3 Horizontal";

            var path = ImageUtils.GenerateBattleshipGameBoard(shipBlocks, Array.Empty<Vector2>(), true, false);
            await interaction.RespondWithFileAsync(path,
                text: reply, ephemeral: true);

            // start attacking phase
            Vector2[] enemyShipBlocks = Array.Empty<Vector2>();
            foreach (var ship in player.ships)
                enemyShipBlocks = enemyShipBlocks.Concat(ship.blocks).ToArray();

            if (hostPlayer.currentPlacingShip == null && otherPlayer.currentPlacingShip == null)
            {
                gamePhase = GamePhase.Attacking;

                if (turn)
                {
                    var board = ImageUtils.GenerateBattleshipGameBoard(shipBlocks, hostPlayer.attacksReceived.ToArray(), true, false);
                    var enemyBoard = ImageUtils.GenerateBattleshipGameBoard(enemyShipBlocks, otherPlayer.attacksReceived.ToArray(), false, true);
                    FileAttachment[] attachments = new FileAttachment[] { new(enemyBoard), new(board) };

                    await hostPlayer.interaction.FollowupWithFilesAsync(attachments, "It is your turn to attack!\nEx.: /battleship attack A3" +
                        "\n:blue_square: - Your board\n:red_square: - Board of the opponent", ephemeral: true);
                }
            }
        }

        public async void AttackOpponentShip(SocketInteraction interaction, Vector2 pos)
        {
            if (gamePhase != GamePhase.Attacking) return;

            Player playerAttacking = interaction.User.Id == hostPlayer.user.Id ? hostPlayer : otherPlayer;
            Player playerAttacked = interaction.User.Id == hostPlayer.user.Id ? otherPlayer : hostPlayer;

            if(turn && playerAttacking.user.Id != hostPlayer.user.Id)
            {
                await interaction.RespondAsync("It is not your turn to play.");
                return;
            }
            else if(!turn && playerAttacking.user.Id != otherPlayer.user.Id)
            {
                await interaction.RespondAsync("It is not your turn to play.");
                return;
            }

            if (pos.x > 10 || pos.y > 10 || HasPlayerSideAttackInBlock(playerAttacked, pos))
            {
                await interaction.RespondAsync("This position is not valid or has already been attacked!", ephemeral: true);
                return;
            }

            if(HasShipInBlock(playerAttacked, pos))
                await interaction.RespondAsync("You got a hit!", ephemeral: true);
            else
                await interaction.RespondAsync("You missed!", ephemeral: true);

            playerAttacked.attacksReceived.Add(pos);
            turn = !turn;

            // now the attacked player can attack
            Vector2[] shipBlocks = Array.Empty<Vector2>();
            foreach (var ship in playerAttacked.ships)
                shipBlocks = shipBlocks.Concat(ship.blocks).ToArray();

            Vector2[] enemyShipBlocks = Array.Empty<Vector2>();
            foreach (var ship in playerAttacking.ships)
                enemyShipBlocks = enemyShipBlocks.Concat(ship.blocks).ToArray();

            var board = ImageUtils.GenerateBattleshipGameBoard(shipBlocks, playerAttacked.attacksReceived.ToArray(), true, false);
            var enemyBoard = ImageUtils.GenerateBattleshipGameBoard(enemyShipBlocks, playerAttacking.attacksReceived.ToArray(), false, true);
            FileAttachment[] attachments = new FileAttachment[] { new(enemyBoard), new(board) };

            await playerAttacked.interaction.FollowupWithFilesAsync(attachments, "It is your turn to attack!\nEx.: /battleship attack A3" +
                "\n:blue_square: - Your board\n:red_square: - Board of the opponent", ephemeral: true);
        }

        // only ment to debug
        public async void AutoFinishPlayerShips(SocketInteraction interaction, Vector2 pos, bool respond)
        {
            if (gamePhase != GamePhase.Placing) return;

            Player player = interaction.User.Id == hostPlayer.user.Id ? hostPlayer : otherPlayer;

            if (player.availableShips == null || player.availableShips.Count == 0)
            {
                await interaction.RespondAsync("You already placed all your ships!", ephemeral: true);
                return;
            }

            List<Vector2> blocks = new();

            for (int i = 0; i < player.currentPlacingShip.length; i++)
            {
                Vector2 block = new(pos.x, pos.y + i);

                if (block.x > 10 || block.y > 10 || HasShipInBlock(player, block))
                {
                    await interaction.RespondAsync("This position is not valid!", ephemeral: true);
                    return;
                }

                blocks.Add(block);
            }

            player.currentPlacingShip.blocks = blocks.ToArray();

            player.ships.Add(player.availableShips[0]);
            player.availableShips.RemoveAt(0);
            player.currentPlacingShip = player.availableShips.Count != 0 ? player.availableShips[0] : null;

            Vector2[] shipBlocks = Array.Empty<Vector2>();
            foreach (var ship in player.ships)
                shipBlocks = shipBlocks.Concat(ship.blocks).ToArray();

            if (respond)
            {
                string reply = player.currentPlacingShip == null ? "You have finished placing your ships, wait for your opponent."
                    : $"Please place your ship. The length is { player.currentPlacingShip.length }.\nEx.: / battleship place A3 Horizontal";

                var path = ImageUtils.GenerateBattleshipGameBoard(shipBlocks, Array.Empty<Vector2>(), true, false);
                await interaction.RespondWithFileAsync(path,
                    text: reply, ephemeral: true);
            }

            // start attacking phase
            Vector2[] enemyShipBlocks = Array.Empty<Vector2>();
            foreach (var ship in player.ships)
                enemyShipBlocks = enemyShipBlocks.Concat(ship.blocks).ToArray();

            if (hostPlayer.currentPlacingShip == null && otherPlayer.currentPlacingShip == null)
            {
                gamePhase = GamePhase.Attacking;

                if (turn)
                {
                    var board = ImageUtils.GenerateBattleshipGameBoard(shipBlocks, hostPlayer.attacksReceived.ToArray(), true, false);
                    var enemyBoard = ImageUtils.GenerateBattleshipGameBoard(enemyShipBlocks, otherPlayer.attacksReceived.ToArray(), false, true);
                    FileAttachment[] attachments = new FileAttachment[] { new(enemyBoard), new(board) };

                    await hostPlayer.interaction.FollowupWithFilesAsync(attachments, "It is your turn to attack!\nEx.: /battleship attack A3" +
                        "\n:blue_square: - Your board\n:red_square: - Board of the opponent", ephemeral: true);
                }
            }
        }


        public static bool HasShipInBlock(Player player, Vector2 pos)
        {
            foreach (var ship in player.ships)
            {
                var block = ship.blocks.Where(x => x.x == pos.x && x.y == pos.y).FirstOrDefault();
                if (block != default) return true;
            }

            return false;
        }

        public static bool HasPlayerSideAttackInBlock(Player player, Vector2 pos)
        {
            var attack = player.attacksReceived.Where(x => x.x == pos.x && x.y == pos.y).FirstOrDefault();
            if (attack != default) return true;

            return false;
        }

        static Ship[] CreateAvailableShips()
        {
            List<Ship> availableShips = new();

            availableShips.Add(new()
            {
                length = 5
            });

            availableShips.Add(new()
            {
                length = 4
            });

            availableShips.Add(new()
            {
                length = 3
            });

            availableShips.Add(new()
            {
                length = 3
            });

            availableShips.Add(new()
            {
                length = 2
            });

            return availableShips.ToArray();
        }

        public class Player
        {
            public Player(IUser user, SocketInteraction interaction)
            {
                this.user = user;
                this.interaction = interaction;
            }

            public IUser user;
            public SocketInteraction interaction;

            public List<Vector2> attacksReceived = new();
            public List<Ship> ships = new();

            public List<Ship> availableShips;
            public Ship currentPlacingShip;
        }

        public class Ship
        {
            public Vector2[] blocks;
            public int length;
            public bool isDestroyed;

            // TODO: image, etc.
        }

        public enum ShipOrientation
        {
            Horizontal,
            Vertical
        }

        public enum GamePhase
        {
            Placing,
            Attacking
        }
    }
}
