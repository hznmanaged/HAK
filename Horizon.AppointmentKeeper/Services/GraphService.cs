using Horizon;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Security.Cryptography.Pkcs;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Horizon.AppointmentKeeper.Exceptions;
using System.Windows.Media.Converters;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Horizon.AppointmentKeeper.Services
{
    public class GraphService : ICalendarSource
    {
        string[] scopes = new string[] { "Calendars.Read" };
        private readonly SettingsContext settings;
        string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";
        private readonly StorageCreationProperties storageProperties;

        public GraphService(SettingsContext settings)
        {
            this.settings = settings;

            var userFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HAK");
            System.IO.Directory.CreateDirectory(userFolder);

            storageProperties =
                new StorageCreationPropertiesBuilder("msal.cache", userFolder)
                .Build();

        }


        public async Task Test()
        {
            var client = await this.GetClient();
            await client.Me.Calendars.Request().GetResponseAsync();
        }



        public async Task<CalendarEvent> GetNextCalendarEvent(GraphServiceClient client, DateTimeOffset? currentTime = null)
        {
            if (!currentTime.HasValue)
            {
                currentTime = DateTimeOffset.Now;
            }

            var calendar = await GetCurrentCalendar(client);

            var queryOptions = new List<QueryOption>()
            {
                new QueryOption("startDateTime", currentTime.Value.ToUniversalTime().ToString("o")),
                new QueryOption("endDateTime", currentTime.Value.AddDays(7).ToUniversalTime().ToString("o"))
            };

            var events = (await client.Me.Calendars[calendar.Id].CalendarView.Request(queryOptions)
                .Filter($"isCancelled eq false")                
                .OrderBy("start/dateTime")
                .GetAsync()).ToList();
            if(!events.Any())
            {
                throw new NoCalenderEventsException($"Calendar {calendar.Name} contains no events");
            }

            var firstEvent = events.First();
            var i = 1;
            var nextEvent = events.Skip(i).FirstOrDefault();
            while (nextEvent != null &&
                nextEvent.Start.ToDateTimeOffset() < currentTime)
            {
                nextEvent = events.Skip(i).FirstOrDefault();
                i++;
            }

            var output = CreateCalenderEvent(firstEvent);
            output.NextEvent = CreateCalenderEvent(nextEvent);

            return output;

        }

        private CalendarEvent CreateCalenderEvent(Event graphEvent)
            => new CalendarEvent()
            {
                ID = graphEvent.Id,
                StartTime = graphEvent.Start.ToDateTimeOffset(),
                EndTime = graphEvent.End.ToDateTimeOffset(),
                Name = graphEvent.Subject
            };
        public async Task<IEnumerable<Calendar>> GetCalendars(GraphServiceClient client)
        {
            return await client.Me.Calendars.Request().GetAsync();
        }

        public async Task<Calendar> GetCurrentCalendar(GraphServiceClient client)
        {
            Calendar calendar = null;
            if (!String.IsNullOrWhiteSpace(settings.GraphCalendar))
            {
                calendar = await client.Me.Calendars[settings.GraphCalendar].Request().GetAsync();

            }
            if (calendar == null)
            {

                var calendars = await client.Me.Calendars.Request().GetAsync();
                if (!calendars.Any())
                {
                    throw new CalendarNotFoundException("No calendars found");
                }
                calendar = calendars.First();
            }


            settings.GraphCalendar = calendar.Id;

            return calendar;
        }

        private IPublicClientApplication pca = PublicClientApplicationBuilder
            .Create(Constants.GRAPH_CLIENT_ID)
            .WithAuthority(AzureCloudInstance.AzurePublic, "common")
            .WithDefaultRedirectUri()
            .Build();

        MsalCacheHelper cacheHelper = null;
        public async Task<GraphServiceClient> GetClient()
        {
            if (cacheHelper == null)
            {
                cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
                cacheHelper.RegisterCache(pca.UserTokenCache);
            }
            //https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=CS#interactive-provider
            return new GraphServiceClient(new DelegateAuthenticationProvider(async (request) =>
            {
                var log = new StringBuilder();
                var user = System.Security.Principal.WindowsIdentity.GetCurrent();
                foreach(var claim in user.Claims)
                {
                    Console.Out.WriteLine();
                }


                var accounts = await pca.GetAccountsAsync();
                AuthenticationResult authResult = null;

                if (accounts.Any())
                {
                    var firstAccount = accounts.FirstOrDefault();
                    try
                    {
                        log.AppendLine($"Attempting silent auth for account {firstAccount.Username}");
                        authResult = await pca.AcquireTokenSilent(scopes, firstAccount)
                            .ExecuteAsync();
                    }
                    catch (MsalUiRequiredException ex)
                    {
                        log.AppendLine($"Silent auth failed: " + ex.Format());
                        // A MsalUiRequiredException happened on AcquireTokenSilent.
                        // This indicates you need to call AcquireTokenInteractive to acquire a token
                        System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                        try
                        {
                            log.AppendLine("Attempting interactive auth");
                            authResult = await pca
                                .AcquireTokenInteractive(scopes)
                                .WithAccount(firstAccount)
                                .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                                .ExecuteAsync();

                        }
                        catch (MsalException msalex)
                        {
                            log.AppendLine($"Interactive auth failed: " + msalex.Format());
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error Acquiring Token Silently:{System.Environment.NewLine}{ex.Format()}");
                    }
                }
                if (authResult == null)
                {
                    try
                    {
                        log.AppendLine("Attempting integrated Windows auth");
                        authResult = await pca
                            .AcquireTokenByIntegratedWindowsAuth(scopes)
                            .ExecuteAsync();
                    }
                    catch (MsalException ex)
                    {
                        log.AppendLine($"Integrated auth failed: " + ex.Format());

                        try
                        {
                            log.AppendLine("Attempting interactive auth");
                            authResult = await pca
                                .AcquireTokenInteractive(scopes)
                                .WithPrompt(Microsoft.Identity.Client.Prompt.SelectAccount)
                                .ExecuteAsync();
                        }
                        catch (MsalException msalex)
                        {
                            log.AppendLine($"Interactive auth failed: " + msalex.Format());
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Integrated auth failed:{System.Environment.NewLine}{ex.Format()}");
                    }
                }


                if (authResult != null)
                {
                    settings.GraphUser = authResult.Account.Username;
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                }
                else
                {
                    throw new Exception("Unable to authenticate against graph. Attempt log: \r\n" + log.ToString());
                }
            }));
        }



        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public async Task ClearAuthCache()
        {
            var accounts = (await pca.GetAccountsAsync()).ToList();

            // clear the cache
            while (accounts.Any())
            {
                await pca.RemoveAsync(accounts.First());
                accounts = (await pca.GetAccountsAsync()).ToList();
            }



        }
    }
}
