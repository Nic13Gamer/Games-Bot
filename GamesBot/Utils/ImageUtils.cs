﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;

namespace GamesBot.Utils
{
    public static class ImageUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shipBlocks"></param>
        /// <param name="attacks"></param>
        /// <returns>The path of the resulting image</returns>
        public static string GenerateBattleshipGameBoard(Vector2[] shipBlocks, Vector2[] attacks)
        {
            using(Image image = Image.Load("Resources/BattleshipGameBoard.png"))
            {
                foreach (var shipBlock in shipBlocks)
                {
                    var blockImg = new Image<Rgba32>(100, 100, Color.Gray);
                    image.Mutate(x => x.DrawImage(blockImg, shipBlock.ToPoint() * 100, 1f));
                }

                foreach (var attackBlock in attacks)
                {
                    Color color = Color.Blue;
                    if (shipBlocks.Where(x => x == attackBlock).FirstOrDefault() != default)
                        color = Color.Red;

                    var blockImg = new Image<Rgba32>(100, 100, Color.Gray);
                    image.Mutate(x => x.DrawImage(blockImg, attackBlock.ToPoint() * 100, 1f));
                }

                string randomString = StringUtils.RandomString(5);
                image.Save("Resources/Out/Temp/battleship-board-" + randomString + ".png");

                return "Resources/Out/Temp/battleship-board-" + randomString + ".png";
            }
        }
    }
}
