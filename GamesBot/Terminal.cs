using GamesBot.Utils;

using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GamesBot
{
    public static class Terminal
    {
        static readonly string path = Bot.DATA_PATH + "/log.txt";
        static readonly List<string> consoleLog = new();

        public static void Start()
        {
            string logStart = $"LOG DATE (LOCAL PC): {DateTime.Now}" +
                $"\nLOG DATE (UTC): {DateTime.UtcNow}" +
                $"\nORIGINAL LOG PATH: {path}" +
                "\n\n\n--- LOG START ---\n";

            consoleLog.Add(logStart + "\n");

            
            Thread thread = new(async () =>
            {
                while (true)
                {
                    try
                    {
                        string[] args = Console.ReadLine().Split(" ");

                        switch (args[0].ToLower())
                        {
                            case "bot_set_game":
                                {
                                    string text = StringUtils.GetAllRemainderTextAfter(args, 0);

                                    await Bot.Client.SetGameAsync(text);

                                    WriteLine("Set bot game to: " + text, MessageType.CMD);

                                    break;
                                }

                            case "guilds_notify":
                                {
                                    string text = StringUtils.GetAllRemainderTextAfter(args, 0);

                                    foreach (var guild in Bot.Client.Guilds)
                                        await guild.DefaultChannel.SendMessageAsync(text);

                                    WriteLine($"Notified {Bot.Client.Guilds.Count} guilds with: {text}", MessageType.CMD);

                                    break;
                                }

                            case "guild_channel_send":
                                {
                                    string channelId = args[1];
                                    string text = StringUtils.GetAllRemainderTextAfter(args, 1);
                                    var channel = Bot.Client.GetChannel(ulong.Parse(channelId));

                                    await (channel as SocketTextChannel).SendMessageAsync(text);

                                    WriteLine($"Send to channel id ({channelId}): {text}", MessageType.CMD);

                                    break;
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLine(e.Message, MessageType.CMD);
                    }
                }
            });
            thread.Start();
        }

        public static void WriteLine(object msg, MessageType type = MessageType.INFO, ConsoleColor color = ConsoleColor.White)
        {
            AddToLog(msg.ToString(), type);

            if (type == MessageType.INFO)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"[{type}] ");
            } else if (type == MessageType.WARN)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{type}] ");
            } else if (type == MessageType.ERROR)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"[{type}] ");
            } else if (type == MessageType.CMD)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"[{type}] ");
            } else if (type == MessageType.API)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"[{type}] ");
            }

            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;

            SaveLog();
        }

        public static void AddToLog(string msg, MessageType type)
        {
            consoleLog.Add($"[{type}] {msg}\n");
        }

        public static void SaveLog()
        {
            try
            {
                string log = string.Empty;

                foreach(string line in consoleLog)
                    log += line;

                File.WriteAllText(path, log);
            }
            catch (Exception e)
            {
                WriteLine(e.Message, MessageType.ERROR);
            }
        }

        public enum MessageType
        {
            INFO,
            WARN,
            ERROR,
            CMD,
            API
        }
    }
}
