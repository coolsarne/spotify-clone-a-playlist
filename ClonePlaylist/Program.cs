using System.Net.Http.Headers;

namespace ClonePlaylist;

public static class Program
{
    private static async Task Main()
    {
        var spotifySettings = Configuration.GetSpotifySettings();
        var accessToken = await Authorize.AuthorizeUserAsync(spotifySettings);
        if (string.IsNullOrEmpty(accessToken))
        {
            Console.WriteLine("Authorization failed or was cancelled.");
            return;
        }
        
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var user = await SpotifyApi.GetCurrentUserIdAsync(httpClient);
        Console.WriteLine(user);
    }
}