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
        const int REQUESTDAYS = 9;

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

        public async Task ListAllEvents(SocketTextChannel channel)
        {

            await channel.DeleteMessagesAsync(channel.GetMessagesAsync().FlattenAsync<IMessage>().Result);
            CombinedEventList = new List<Events>();
            eventItems = new List<Event>();
            string returnString = string.Empty;
            // List events.
            foreach (string c in calendars)
            {
                CombinedEventList.Add(CreateEventsList(c));
            }
            DateTime currentDate = DateTime.Today.AddDays(REQUESTDAYS);
            // Sort the CombinedEventList (hopefully by date)
            for (int i = REQUESTDAYS; i >= 0; i++)
            {
                List<Event> today = new List<Event>();
                foreach (Events events in CombinedEventList)
                {
                    if (events.Items != null && events.Items.Count > 0)
                    {
                        foreach (Event item in events.Items)
                        {
                            if (item.Start.Date != null
                                && DateTime.TryParseExact(item.Start.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)
                                && dt.ToString("yyyy-MM-dd") == currentDate.ToString("yyyy-MM-dd"))
                            {
                                today.Add(item);
                            }
                            else if (item.Start.DateTime.ToString() == currentDate.ToString())
                            {
                                today.Add(item);
                            }


                            Console.Out.WriteLine(item.Start.DateTime.ToString());
                        }
                    }
                }
                if (today.Count > 0)
                {
                    await SendChannelMessageAsync(today, channel, currentDate);
                }
                currentDate = currentDate.AddDays(-1);
            }

        }

        public async Task SendChannelMessageAsync(List<Event> events, SocketTextChannel channel, DateTime date)
        {
            // calendar Channel ID: 504509810755239936
            //await messages.FlattenAsync<IMessage>();
            foreach (Event e in events)
            {
                var builder = new EmbedBuilder()
                {
                    Color = Color.Blue,
                    Title = $"{date.DayOfWeek}",
                }
                //.WithFooter(footer => footer.Text = $"{this.CharacterDescription}")
                //    foreach(Event e in events)
                //{
                .AddField("Event Info: ", $"Summary: {e.Summary}\n" +
                    $"Location: {e.Location}\n" +
                    $"Event Link: {e.HtmlLink}\n" +
                    $"Start and End: {e.Start.Date.ToString()} to {e.End.Date.ToString()}\n");
                await channel.SendMessageAsync($"***{date.ToShortDateString()}***", false, builder.Build());
            }
            //.AddField("Description: ", $"{}");

            //await channel.SendMessageAsync($"***{date.ToShortDateString()}***", false, builder.Build());
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

    }
}
