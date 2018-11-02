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

        // Specific to plugins
        // TODO: Find a better way to handle these
        Calendar calendar = new Calendar();


        public static void Main(string[] args)
                    => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {



            string dateString = "2018-10-31";
            bool x = DateTime.TryParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dt);

                

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

                if (substring.Length >= 1)
                {
                    switch(substring[0])
                    {
                        case "calendar":
                            //await message.Channel.SendMessageAsync($"```{calendar.ListAllEvents()}```");
                            await calendar.ListAllEvents(message);
                            break;
                        case "help":
                            await message.Channel.SendMessageAsync("Current commands:\n\t``^calendar`` for SLCC calendar events\n\t``^help``");
                            break;
                        default:
                            await message.Channel.SendMessageAsync("Current commands:\n\t``^calendar`` for SLCC calendar events\n\t``^help``");
                            break;
                    }
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
