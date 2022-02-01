using Discord;
using Discord.WebSocket;
using GamesBot.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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
            var path = ImageUtils.GenerateBattleshipGameBoard(new Vector2[] {pos }, Array.Empty<Vector2>());
            await interaction.RespondWithFileAsync(path);
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
