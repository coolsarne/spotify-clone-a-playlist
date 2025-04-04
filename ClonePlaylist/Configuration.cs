using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ClonePlaylist;

public static class Configuration
{
    public static SpotifySettings GetSpotifySettings()
    {
        IConfiguration config;
        try
        {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Error: appsettings.json not found. Please create it in the project directory.");
            throw;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error reading appsettings.json: {ex.Message}");
            throw;
        }
        
        var spotifySettings = config.GetSection("Spotify").Get<SpotifySettings>();
        
        if (
            spotifySettings is null ||
            string.IsNullOrEmpty(spotifySettings.ClientId) ||
            string.IsNullOrEmpty(spotifySettings.ClientSecret) ||
            string.IsNullOrEmpty(spotifySettings.RedirectUri))
        {
            Console.WriteLine("Error: Spotify ClientId, ClientSecret, or RedirectUri missing or empty in appsettings.json.");
            throw new();
        }
        
        return spotifySettings;
    }
}