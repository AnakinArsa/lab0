using System;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleCrawler
{
    public class Crawler
    {
        protected string? basedFolder = null;
        protected int maxLinksPerPage = 3;

        // Method to set the base folder where content will be stored
        public void SetBasedFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }
            basedFolder = folder;
        }

        // Method to set the maximum number of links to crawl per page
        public void SetMaxLinksPerPage(int max)
        {
            maxLinksPerPage = max;
        }

        // The core method to download a page and crawl links recursively
        public async Task GetPage(string url, int level)
        {
            if (basedFolder == null)
            {
                throw new Exception("Please set the base folder using SetBasedFolder method first.");
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            // Simple HttpClient to fetch content from the URL
            HttpClient client = new();
            try
            {
                // Get the content from the URL
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Reformat URL to create a valid filename
                    string fileName = url.Replace(":", "_").Replace("/", "_").Replace(".", "_") + ".html";
                    // Save the content to a file
                    File.WriteAllText(basedFolder + "/" + fileName, responseBody);

                    // If we've reached the max depth (level 0), stop recursion
                    if (level <= 0) return;

                    // Extract links from the page content
                    List<string> links = GetLinksFromPage(responseBody);
                    int count = 0;
                    // Loop through each link and recursively download it
                    foreach (string link in links)
                    {
                        // Only follow http/https links
                        if (link.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (++count > maxLinksPerPage) break; // Stop if we hit the max link limit
                            // Recursively get the linked pages
                            await GetPage(link, level - 1);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to load page: {0}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        // Method to extract links using a simple regular expression
        public static List<string> GetLinksFromPage(string content)
        {
            List<string> links = new();
            Regex regex = new Regex(@"href\s*=\s*""(http[s]?://[^\s""]+)""", RegexOptions.IgnoreCase);
            var matches = regex.Matches(content);

            foreach (Match match in matches)
            {
                links.Add(match.Groups[1].Value);
            }

            return links;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Crawler crawler = new();
            crawler.SetBasedFolder("./"); // Set folder to store pages
            crawler.SetMaxLinksPerPage(3); // Limit to 3 links per page

            // Start crawling from a hardcoded URL with a depth of 2
            await crawler.GetPage("https://dandadan.net/", 2); // Change URL as needed
        }
    }
}

