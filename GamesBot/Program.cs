using System;

namespace GamesBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot.Start().GetAwaiter().GetResult();
        }
    }
}
