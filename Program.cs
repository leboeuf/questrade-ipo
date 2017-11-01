using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;

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
        private static HtmlParser _htmlParser = new HtmlParser();
        private static HttpClient _httpClient = new HttpClient();
        
        static async Task Main(string[] args)
        {
            // var json = await _httpClient.GetStringAsync("http://www.questrade.com/Sitefinity/Public/Services/Questrade/Services/IPOService.svc/GetIPOList");
            // await File.WriteAllTextAsync("/Users/node/Desktop/ipos.json", json);
            var json = await File.ReadAllTextAsync("/Users/node/Desktop/ipos.json");
            
            var ipoList = JsonConvert.DeserializeObject<IEnumerable<IpoListItem>>(json);
            var ipos = ipoList.Where(i => i.Category == "Equity" && i.Status == "Closed")
                .OrderBy(i => i.DateAdded)
                .ToList();

            for (var i = 0; i < ipos.Count; i++)
            {
                var ipo = ipos[i];
                // var ipoJson = await _httpClient.GetStringAsync($"http://www.questrade.com/Sitefinity/Public/Services/Questrade/Services/IPOService.svc/GetIPODetails/{ipo.IpoURL}/");
                // await File.WriteAllTextAsync($"/Users/node/Desktop/ipos/{ipo.IpoURL}.json", ipoJson);
                // Console.WriteLine($"{i}/{ipos.Count - 1} ({ipo.IpoURL})");
                var ipoJson = await File.ReadAllTextAsync($"/Users/node/Desktop/ipos/{ipo.IpoURL}.json");
                var ipoItem = JsonConvert.DeserializeObject<Ipo>(ipoJson);

                var regex = new Regex("under the symbol “(.*)”.", RegexOptions.IgnoreCase);
                var matches = regex.Matches(ipoItem.SummeryDetails);
                if (!matches.Any())
                {
                    continue;
                }

                var symbol = matches[0].Groups[1].Value;
                Console.WriteLine($"{ipoItem.DateAdded}\t{symbol}\t{ipoItem.Industry}\t{ipoItem.IssuerName}");
            }
        }
    }
}
