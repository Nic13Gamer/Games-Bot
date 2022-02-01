using Discord;
using Discord.WebSocket;
using GamesBot.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GamesBot.Games
{
    public class BattleshipGame : Game
    {
        public readonly Player hostPlayer;
        public readonly Player otherPlayer;

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

            await hostPlayer.interaction.FollowupWithFileAsync(Directory.GetCurrentDirectory() + "/Resources/BattleshipGameBoard.png",
                text: $"Please place your ship. The length is {ship.length}.\nEx.: /battleship place A3 Horizontal", ephemeral: true);
            await otherPlayer.interaction.FollowupWithFileAsync(Directory.GetCurrentDirectory() + "/Resources/BattleshipGameBoard.png",
                text: $"Please place your ship. The length is {ship.length}.\nEx.: /battleship place A3 Horizontal", ephemeral: true);

            hostPlayer.currentPlacingShip = ship;
            otherPlayer.currentPlacingShip = ship;
        }

        public async void PlacePlayerShip(SocketInteraction interaction, Vector2 pos, ShipOrientation orientation)
        {
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
            foreach (var ship in otherPlayer.ships)
                shipBlocks = shipBlocks.Concat(ship.blocks).ToArray();

            string reply = player.currentPlacingShip == null ? "You have finished placing your ships, wait for your opponent."
                : $"Please place your ship.The length is { player.currentPlacingShip.length }.\nEx.: / battleship place A3 Horizontal";

            var path = ImageUtils.GenerateBattleshipGameBoard(shipBlocks, Array.Empty<Vector2>());
            await interaction.RespondWithFileAsync(path,
                text: reply, ephemeral: true);
        }

        static bool HasShipInBlock(Player player, Vector2 pos)
        {
            foreach (var ship in player.ships)
            {
                var block = ship.blocks.Where(x => x.x == pos.x && x.y == pos.y).FirstOrDefault();
                if (block != default) return true;
            }

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

            public List<Vector2> attacks = new();
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
    }
}
