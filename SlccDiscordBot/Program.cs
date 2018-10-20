using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SlccDiscordBot.classes;

namespace SlccDiscordBot
{
    class Program
    {
        DiscordSocketClient client;
        BotConfig botConfig = new BotConfig();


        public static void Main(string[] args)
                    => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient
            (new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
                //LogLevel = LogSeverity.Verbose
                //LogLegel = LogSeverity.Info
            });

            SetUp(ref botConfig);

            client.Log += Log;
            string token = botConfig.Token;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            client.MessageReceived += MessageReceived;

            await Task.Delay(-1);

        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content.StartsWith("^"))
            {
                string msg = message.Content.Substring(1).ToLower();

                string[] substring = msg.Split(" ", 2);

                if (substring.Length != 1)
                {
                    await message.Channel.SendMessageAsync($"I'm still being made. I don't do anything cool yet.\nCheck me out at: https://github.com/Zucce05/SlccDiscordBot");
                }
                else
                {
                    await message.Channel.SendMessageAsync($"I'm still being made. I don't do anything cool yet.\nCheck me out at: https://github.com/Zucce05/SlccDiscordBot");
                }
            }
        }


        
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static void SetUp(ref BotConfig bc)
        {
            JsonTextReader reader;
            //try
            //{
            //    // This is good for development where I've got the config with the project
            //    reader = new JsonTextReader(new StreamReader("..\\..\\..\\json\\BotConfig.json"));
            //    bc = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("..\\..\\..\\json\\BotConfig.json"));
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine($"Project Level SetUp Exception:\n\t{e.Message}");
            //}
            try
            {
                // This is good for deployment where I've got the config with the executable
                reader = new JsonTextReader(new StreamReader("json\\BotConfig.json"));
                bc = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("json\\BotConfig.json"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Executable BotConfig SetUp Exception:\n\t{e.Message}");
            }
        }
    }
}
