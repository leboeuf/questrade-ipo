using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace questrade_ipo
{
    public class IpoListItem
    {
        public string Category { get; set; }
        public DateTime DateAdded { get; set; }
        public string Industry { get; set; }
        public string IpoURL { get; set; }
        public string IssueType { get; set; }
        public string IssuerName { get; set; }
        public bool NotAvailable { get; set; }
        public string NotAvailableText { get; set; }
        public string Status { get; set; }
    }

    public class Ipo : IpoListItem
    {
        public object[] Documents { get; set; }
        public string Jurisdiction { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MinimumSubscription { get; set; }
        public string Pricing { get; set; }
        public string RegisteredPlanEligibility { get; set; }
        public string SummeryDetails { get; set; }
    }

    class Program
    {
        private const string APIURL = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&apikey={0}&datatype=csv&symbol=TSX:{1}";
        private const string APIKEY = ""; // Put your API key here
        private static HtmlParser _htmlParser = new HtmlParser();
        private static HttpClient _httpClient = new HttpClient();
        
        static async Task Main(string[] args)
        {
            if (APIKEY == string.Empty)
            {
                Console.WriteLine("You must set your API key. Aborting.");
                return;
            }

            var json = await _httpClient.GetStringAsync("http://www.questrade.com/Sitefinity/Public/Services/Questrade/Services/IPOService.svc/GetIPOList");
            // await File.WriteAllTextAsync("/Users/node/Desktop/ipos.json", json);
            // var json = await File.ReadAllTextAsync("/Users/node/Desktop/ipos.json");
            
            var ipoList = JsonConvert.DeserializeObject<IEnumerable<IpoListItem>>(json);
            var ipos = ipoList.Where(i => i.Category == "Equity" && i.Status == "Closed")
                .OrderBy(i => i.DateAdded)
                .ToList();

            for (var i = 0; i < ipos.Count; i++)
            {
                var ipo = ipos[i];
                var ipoJson = await _httpClient.GetStringAsync($"http://www.questrade.com/Sitefinity/Public/Services/Questrade/Services/IPOService.svc/GetIPODetails/{ipo.IpoURL}/");
                // await File.WriteAllTextAsync($"/Users/node/Desktop/ipos/{ipo.IpoURL}.json", ipoJson);
                // var ipoJson = await File.ReadAllTextAsync($"/Users/node/Desktop/ipos/{ipo.IpoURL}.json");
                var ipoItem = JsonConvert.DeserializeObject<Ipo>(ipoJson);

                var regex = new Regex("under the symbol “(.*)”.", RegexOptions.IgnoreCase);
                var matches = regex.Matches(ipoItem.SummeryDetails);
                if (!matches.Any())
                {
                    continue;
                }

                var symbol = matches[0].Groups[1].Value;
                if (symbol.Length > 6)
                {
                    // Quick fix for unclean matches that grab more than just the symbol because of special characters
                    continue;
                }

                var apiResponse = await _httpClient.GetStringAsync(string.Format(APIURL, APIKEY, symbol));
                if (apiResponse.Contains("Error"))
                {
                    // If first character is \n there was an error (API has no data for this symbol)
                    continue;
                }

                var allLines = apiResponse.Split("\r\n")
                    .Skip(1) // Remove the header row
                    .Reverse().Skip(1).Reverse(); // Remove the last (empty) element
                var parsedCsv = allLines.Take(1) // Newest data point
                .Union(allLines.Reverse().Take(1)) // Oldest data point
                .Select(line => line.Split(','))
                .Select(csv => new
                {
                    Date = csv[0],
                    Open = double.Parse(csv[1]),
                    High = double.Parse(csv[2]),
                    Low = double.Parse(csv[3]),
                    Close = double.Parse(csv[5]), // Adjusted close
                    Volume = int.Parse(csv[6]),
                }).ToList();

                if (parsedCsv.Count > 2)
                {
                    continue;
                }

                var profit = Math.Round(parsedCsv[1].Close - parsedCsv[0].Close, 2);
                Console.WriteLine($"{symbol}: {profit}");
            }
        }
    }
}
