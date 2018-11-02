using Discord;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SlccDiscordBot.classes
{
    public class Calendar
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json
        string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        string ApplicationName = "SLCC Bot";
        CalendarService service = new CalendarService();
        UserCredential credential;
        HashSet<string> calendars = new HashSet<string>();
        List<Events> CombinedEventList = new List<Events>();
        List<Event> eventItems = new List<Event>();
        const int REQUESTDAYS = 8;

        public Calendar()
        {
            using (var stream =
                new FileStream("json\\credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            calendars.Add("bruinmail.slcc.edu_tcet8ho01q49t4laddlcbetpqc@group.calendar.google.com");
            calendars.Add("bruinmail.slcc.edu_jhf8br5bds5fmosrr6ivk3o3to@group.calendar.google.com");
            calendars.Add("bruinmail.slcc.edu_3peeg5obg47g233n28p496uhd8@group.calendar.google.com");
            calendars.Add("bruinmail.slcc.edu_3d3j70osj9jnt2gquuqt64b2es@group.calendar.google.com");
            calendars.Add("bruinmail.slcc.edu_cpmd62p4gl29taa34956bj529s@group.calendar.google.com");
            calendars.Add("en.usa#holiday@group.v.calendar.google.com");
        }

        public async Task ListAllEvents(SocketMessage message)
        {
            CombinedEventList = new List<Events>();
            eventItems = new List<Event>();
            string returnString = string.Empty;
            // List events.
            foreach (string c in calendars)
            {
                CombinedEventList.Add(CreateEventsList(c));
            }
            // Sort the CombinedEventList (hopefully by date)
            DateTime currentDays = new DateTime();
            for (int i = 0; i < REQUESTDAYS; i++)
            {
                currentDays = currentDays.AddDays(i);
                List<Event> today = new List<Event>();
                foreach (Events events in CombinedEventList)
                {
                    if (events.Items != null && events.Items.Count > 0)
                    {
                        foreach (Event item in events.Items)
                        {
                            if(item.Start.Date != null
                                && DateTime.TryParseExact(item.Start.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)
                                && dt.ToString("yyyy-MM-dd") == currentDays.ToString("yyyy-MM-dd"))
                            {
                                today.Add(item);
                            }
                            else if(item.Start.DateTime.ToString() == currentDays.ToString("yyyy-MM-dd"))
                            {
                                today.Add(item);
                            }


                            eventItems.Add(item);
                            Console.Out.WriteLine($"Date: {item.Start.Date}");
                            Console.Out.WriteLine($"DateTime: {item.Start.DateTime}");
                            //DateTime.TryParseExact(item.Start.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dt);
                            //Console.Out.WriteLine($"ParsedTime: {dt.ToString("yyyy-MM-dd")}");
                            Console.Out.WriteLine(String.Empty);
                        }
                    }
                }
                await SendChannelMessageAsync(today, message);
            }
                        
            // foreach (Events events in CombinedEventList)
            // {
            //     if (events.Items != null && events.Items.Count > 0)
            //     {
            //         returnString += ($"Calendar Description: {events.Description}\n");
            //         foreach (var eventItem in events.Items)
            //         {
            //             string when = eventItem.Start.DateTime.ToString();
            //             if (String.IsNullOrEmpty(when))
            //             {
            //                 when = eventItem.Start.Date;
            //             }
            //             returnString += ($"\t{eventItem.Summary} ({when})\n");
            //         }
            //         returnString += ("\n");
            //     }
            // }
        }

        public async Task SendChannelMessageAsync(List<Event> events, SocketMessage message)
        {
            // calendar Channel ID: 504509810755239936
            SocketTextChannel test = new SocketTextChannel();

            var builder = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "Tales of Nowhere",
                Description = $"Famous Saying: '{this.CharacterQuote}'",
                ImageUrl = $"{this.ImageUrl}",
                Timestamp = DateTimeOffset.Now,
            }
                            .WithFooter(footer => footer.Text = $"{this.CharacterDescription}")
                            .AddField("Name: ", $"{this.CharacterName}");

            await message.Channel.SendMessageAsync(this.Lore, false, builder.Build());
        }

        private Events CreateEventsList(string calendar)
        {
            EventsResource.ListRequest request = service.Events.List(calendar);
            request.TimeMin = DateTime.Now;
            request.TimeMax = DateTime.Today.AddDays(REQUESTDAYS);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            // request.MaxResults = 10;
            // request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            return request.Execute();
        }

        // private async Task SendChannelMessageAsync(SocketMessage message)
        // {
        //     var builder = new EmbedBuilder()
        //     {
        //         Color = Color.Blue,
        //         Title = "Tales of Nowhere",
        //         Description = $"Famous Saying: '{this.CharacterQuote}'",
        //         ImageUrl = $"{this.ImageUrl}",
        //         Timestamp = DateTimeOffset.Now,
        //     }
        //                     .WithFooter(footer => footer.Text = $"{this.CharacterDescription}")
        //                     .AddField("Name: ", $"{this.CharacterName}");

        //     await message.Channel.SendMessageAsync(this.Lore, false, builder.Build());
        // }

    }
}
