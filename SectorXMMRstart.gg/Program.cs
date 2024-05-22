using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
//Written by Cade Gilbert
class Program
{
    static string apiUrl = "https://api.start.gg/gql/alpha";
    static string apiToken = "";
    static List<Player> players = new List<Player>();

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Enter your Start.GG API key:");
            apiToken = Console.ReadLine();

            while (true)
            {
                Console.WriteLine("Enter a command (e.g., 'fetch tournament', 'fetch league', 'print players', 'save players', 'save html', 'exit'):");
                string input = Console.ReadLine().ToLower();

                if (input.StartsWith("fetch tournament"))
                {
                    await FetchTournamentData();
                }
                else if (input.StartsWith("fetch league"))
                {
                    await FetchLeagueData();
                }
                else if (input.StartsWith("print players"))
                {
                    PrintPlayersToFile();
                }
                else if (input.StartsWith("save players"))
                {
                    SavePlayersToFile();
                }
                else if (input.StartsWith("save html"))
                {
                    SaveHtmlToFile();
                }
                else if (input.StartsWith("exit"))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid command. Please try again.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task FetchTournamentData()
    {
        try
        {
            Console.WriteLine("Enter tournament URL:");
            string tournamentUrl = Console.ReadLine();
            string slug = ExtractSlugFromUrl(tournamentUrl);

            if (!string.IsNullOrEmpty(slug))
            {
                await FetchPlayersFromTournament(slug);
            }
            else
            {
                Console.WriteLine("Invalid tournament URL.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while fetching tournament data: {ex.Message}");
        }
    }

    static async Task FetchLeagueData()
    {
        try
        {
            Console.WriteLine("Enter league URL:");
            string leagueUrl = Console.ReadLine();
            string slug = ExtractSlugFromUrl(leagueUrl);

            if (!string.IsNullOrEmpty(slug))
            {
                await FetchPlayersFromLeague(slug);
            }
            else
            {
                Console.WriteLine("Invalid league URL.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while fetching league data: {ex.Message}");
        }
    }

    static string ExtractSlugFromUrl(string url)
    {
        Uri uri;
        if (Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            string[] segments = uri.Segments;
            return segments.Length > 2 ? segments[2].TrimEnd('/') : null;
        }
        return null;
    }

    static async Task FetchPlayersFromTournament(string slug)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            var query = new
            {
                query = @"
                {
                    tournament(slug: """ + slug + @""") {
                        events {
                            id
                            name
                            standings(query: {perPage: 100, page: 1}) {
                                nodes {
                                    entrant {
                                        id
                                        name
                                    }
                                    placement
                                }
                            }
                        }
                    }
                }"
            };

            var jsonContent = JsonConvert.SerializeObject(query);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var graphQLResponse = JsonConvert.DeserializeObject<GraphQLResponse>(result);

                if (graphQLResponse?.Data?.Tournament?.Events != null)
                {
                    foreach (var evt in graphQLResponse.Data.Tournament.Events)
                    {
                        Console.WriteLine($"Event: {evt.Name}");
                        foreach (var node in evt.Standings.Nodes)
                        {
                            players.Add(new Player
                            {
                                Name = node.Entrant.Name,
                                Placement = node.Placement
                            });
                            Console.WriteLine($"Player {node.Entrant.Name}, Placement: {node.Placement}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No events found for this tournament.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching players from tournament: {ex.Message}");
            }
        }
    }

    static async Task FetchPlayersFromLeague(string slug)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            var query = new
            {
                query = @"
                {
                    league(slug: """ + slug + @""") {
                        events {
                            id
                            name
                            standings(query: {perPage: 100, page: 1}) {
                                nodes {
                                    entrant {
                                        id
                                        name
                                    }
                                    placement
                                }
                            }
                        }
                    }
                }"
            };

            var jsonContent = JsonConvert.SerializeObject(query);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var graphQLResponse = JsonConvert.DeserializeObject<GraphQLResponse>(result);

                if (graphQLResponse?.Data?.League?.Events != null)
                {
                    foreach (var evt in graphQLResponse.Data.League.Events)
                    {
                        Console.WriteLine($"Event: {evt.Name}");
                        foreach (var node in evt.Standings.Nodes)
                        {
                            players.Add(new Player
                            {
                                Name = node.Entrant.Name,
                                Placement = node.Placement
                            });
                            Console.WriteLine($"Player {node.Entrant.Name}, Placement: {node.Placement}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No events found for this league.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching players from league: {ex.Message}");
            }
        }
    }

    static void PrintPlayersToFile()
    {
        try
        {
            string filePath = @"C:\Users\cadeg\OneDrive\Desktop\PlayerStats\PlayerData.txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var player in players)
                {
                    writer.WriteLine($"Player: {player.Name}, Placement: {player.Placement}");
                }
            }
            Console.WriteLine("Player data saved to file successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while printing players to file: {ex.Message}");
        }
    }

    static void SavePlayersToFile()
    {
        try
        {
            string directoryPath = @"C:\Users\cadeg\OneDrive\Desktop\PlayerStats";
            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "PlayerData.txt");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var player in players)
                {
                    writer.WriteLine($"Player: {player.Name}, Placement: {player.Placement}");
                }
            }
            Console.WriteLine($"Player data saved to {filePath} successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while saving players to file: {ex.Message}");
        }
    }

    static void SaveHtmlToFile()
    {
        try
        {
            string directoryPath = @"C:\Users\cadeg\OneDrive\Desktop\PlayerStats";
            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "PlayerData.html");

            var htmlContent = GenerateHtmlContent();

            File.WriteAllText(filePath, htmlContent);
            Console.WriteLine($"Player data saved to {filePath} successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while saving HTML to file: {ex.Message}");
        }
    }

    static string GenerateHtmlContent()
    {
        StringBuilder html = new StringBuilder();
        html.Append("<html><head><style>");
        html.Append("body { font-family: Arial, sans-serif; display: flex; justify-content: center; }");
        html.Append(".frame { width: 520px; display: grid; grid-template-columns: repeat(2, 1fr); gap: 10px; }");
        html.Append(".player { color: white; padding: 10px; border: 20px solid white; border-radius: 10px; text-align: center; }");
        html.Append(".player-info { font-size: 12px; }");
        html.Append("</style></head><body>");
        html.Append("<div class='frame'>");

        int maxPlacement = players.Count > 0 ? players.Max(p => p.Placement) : 1;

        foreach (var player in players)
        {
            string color = CalculateColor(player.Placement, maxPlacement);
            html.Append($"<div class='player' style='background-color: {color};'><div class='player-info'>{player.Placement} - {player.Name}</div></div>");
        }

        html.Append("</div></body></html>");
        return html.ToString();
    }

    static string CalculateColor(int placement, int maxPlacement)
    {
        int blueValue = 0x33;
        int greyValue = 0x99;
        double fraction = (double)placement / maxPlacement;

        int red, green, blue;
        if (fraction <= 1.0)
        {
            red = (int)(blueValue + fraction * (greyValue - blueValue));
            green = (int)(blueValue + fraction * (greyValue - blueValue));
            blue = 0xFF - (int)(fraction * (0xFF - greyValue));
        }
        else
        {
            red = greyValue;
            green = greyValue;
            blue = greyValue;
        }

        return $"#{red:X2}{green:X2}{blue:X2}";
    }

    public class GraphQLResponse
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public Tournament Tournament { get; set; }
        public League League { get; set; }
    }

    public class Tournament
    {
        public List<Event> Events { get; set; }
    }

    public class League
    {
        public List<Event> Events { get; set; }
    }

    public class Event
    {
        public string Name { get; set; }
        public Standings Standings { get; set; }
    }

    public class Standings
    {
        public List<Node> Nodes { get; set; }
    }

    public class Node
    {
        public Entrant Entrant { get; set; }
        public int Placement { get; set; }
    }

    public class Entrant
    {
        public string Name { get; set; }
    }

    public class Player
    {
        public string Name { get; set; }
        public int Placement { get; set; }
    }

}