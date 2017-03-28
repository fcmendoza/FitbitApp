using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using RestSharp;
using ConsoleApp.Models;

namespace ConsoleApp
{
    public class FitbitService
    {
        public FitbitService()
        {
            string baseUrl = "https://api.fitbit.com";
            _client = new RestClient(baseUrl);
        }

        public void Start()
        {
            InitializeTokens();

            int daysCount = int.TryParse(ConfigurationManager.AppSettings["DaysCount"], out daysCount) ? daysCount : 14;
            var from = DateTime.Now.AddDays((daysCount) * -1);
            var to = DateTime.Now;

            Console.WriteLine($"Retrieving daily totals from {from:ddd MMM dd} to {to:ddd MMM dd} (last {daysCount} days) ...");

            var entries = GetFoodSummaries(from, to);

            Console.WriteLine("Finished retrieving daily totals. (Average values do not include today's date):\n");

            var lines = new List<string>();
            lines.Add($"Nutrition logs (last {daysCount} days)\r\n");
            lines.Add("```");

            foreach (var entry in entries)
            {
                PrintEntry(entry, lines);
            }

            Console.WriteLine();

            var averages = new FoodSummary
            {
                // exclude today's date from the averages
                Date = DateTime.MinValue,
                CaloriesTotal = entries.Where(x => x.Date.Date != DateTime.Now.Date).Average(x => x.CaloriesTotal), 
                ProteinTotal = entries.Where(x => x.Date.Date != DateTime.Now.Date).Average(x => x.ProteinTotal),
                CarbsTotal = entries.Where(x => x.Date.Date != DateTime.Now.Date).Average(x => x.CarbsTotal),
            };
            
            lines.Add(string.Empty);
            PrintEntry(averages, lines);
            lines.Add("```");

            File.WriteAllLines(ConfigurationManager.AppSettings["NotesFilePath"], lines, System.Text.Encoding.UTF8);  // Save lines to file;

            Console.WriteLine();
        }

        private void PrintEntry(FoodSummary entry, List<string> lines)
        {
            string calories = entry.CaloriesTotal.ToString("##,###").PadRight(5);
            string proteins = entry.ProteinTotal.ToString("##").PadRight(3) + " g";
            string carbs = entry.CarbsTotal.ToString("##").PadRight(3) + " g";

            var dateString = entry.Date > DateTime.MinValue ? $"{entry.Date:ddd MMM dd}" : new string(' ', $"{entry.Date:ddd MMM dd}".Length);

            string percentages = $"{entry.ProteinPercentage:####} % v {entry.CarbsPercentage:####} %";

            if (entry.ProteinGoalAchived)
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (entry.ProteinGoalAchived && entry.ProteinWins) {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (entry.CarbProteinRatioIsHigh)
                Console.ForegroundColor = ConsoleColor.Red;

            string line =
                $"{dateString}   Cal: {calories}   Prot: {proteins}   Carbs: {carbs}   {percentages}";

            Console.WriteLine(line);
            Console.ResetColor();

            lines.Add(line);
        }

        public List<FoodSummary> GetFoodSummaries(DateTime from, DateTime to)
        {
            var summaries = new List<FoodSummary>();

            foreach (var day in EachDay(from, to))
            {
                var summary = GetFoodSummary(day);
                if (summary.CaloriesTotal > 0)
                    summaries.Add(summary);
                else
                    Console.WriteLine($"No foods logged on {day.Date:MMM dd}");
            }

            return summaries;
        }

        private FoodSummary GetFoodSummary(DateTime date)
        {
            Console.WriteLine($"Retrieving totals for {date:MMM dd} ...");

            string url = $"1/user/-/foods/log/date/{date:yyyy-MM-dd}.json";
            var request = new RestRequest(url, Method.GET);
            request.AddHeader("Authorization", $"Bearer {_accessToken}");

            var response = _client.Execute<FoodResponse>(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (response.Content.Contains("expired_token"))
                {
                    Console.WriteLine($"Access Token expired. Refreshing token and retrying operation.");
                    RefreshAccessToken();
                    GetFoodSummary(date); // retry;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK) {
                Console.WriteLine($"An error was returned from Fitbit's API. StatusCode={response.StatusCode} ({response.StatusDescription}), ResponseContent='{response.Content}'.");
            }

            //if(response.StatusCode == HttpStatusCode.Redirect)

            var summary = response.Data.summary;

            return new FoodSummary { Date = date, CaloriesTotal = summary.calories, ProteinTotal = summary.protein, CarbsTotal = summary.carbs };
        }

        #region Token management TODO: Move to another class
        private void InitializeTokens()
        {
            Console.WriteLine($"Retrieving last access and refresh tokens from storage...");

            var lines  = File.ReadAllLines(ConfigurationManager.AppSettings["TokensFilePath"]);
            _accessToken = lines[0];
            _refreshToken = lines[1];

            var settings = new SettingsManager().Retrieve();

            var tokensAreEqual = _accessToken == settings.AccessToken && _refreshToken == settings.RefreshToken;
            Console.WriteLine($"Tokens retrieved. Tokens from disk and remote storage are equal = {tokensAreEqual}.\n");
        }

        private void GetAccessToken()
        {
            string oauthUrl = "oauth2/token";

            var request = new RestRequest(oauthUrl, Method.POST);

            request.AddHeader("Authorization", $"Basic {_base64EncodedCredentials}");
            request.AddParameter("client_id", "228CJ8");
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("redirect_uri", _redirectUrl);
            request.AddParameter("code", _authorizationCode);

            var response = _client.Execute<AccessTokenResponse>(request);
            var content = response.Content;

            var accessTokenResponse = response.Data;

            _accessToken = accessTokenResponse.access_token;
            _refreshToken = accessTokenResponse.refresh_token;

            Console.WriteLine(content);
        }

        private void RefreshAccessToken()
        {
            Console.WriteLine($"Refreshing Access Token...");

            string oauthUrl = "oauth2/token";

            var request = new RestRequest(oauthUrl, Method.POST);

            request.AddHeader("Authorization", $"Basic {_base64EncodedCredentials}");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", _refreshToken);

            var response = _client.Execute<AccessTokenResponse>(request);
            var content = response.Content;

            var accessTokenResponse = response.Data;

            _accessToken = accessTokenResponse.access_token;
            _refreshToken = accessTokenResponse.refresh_token;

            // Update storage with new tokens
            using (var sw = new StreamWriter(ConfigurationManager.AppSettings["TokensFilePath"], false))
            {
                sw.WriteLine($"{_accessToken}\r\n{_refreshToken}");
            }

            // Update storage with new tokens
            new SettingsManager().UpdateSettings(accessToken: _accessToken, refreshToken: _refreshToken);

            Console.WriteLine($"Access Token request.");
            Console.WriteLine(content);
        }

        #endregion

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        private readonly RestClient _client;
        private string _authorizationCode = ConfigurationManager.AppSettings["AuthorizationCode"];
        private string _accessToken;
        private string _refreshToken;
        private string _base64EncodedCredentials = ConfigurationManager.AppSettings["Base64EncodedCredentials"];
        private string _redirectUrl = ConfigurationManager.AppSettings["RedirectUrl"];
    }
}
