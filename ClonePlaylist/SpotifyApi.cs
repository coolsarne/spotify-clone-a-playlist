using System.Text;
using System.Text.Json;

namespace ClonePlaylist;

public static class SpotifyApi
{
    private const string SpotifyApiUrl = "https://api.spotify.com/v1";
    
    public static async Task<string?> GetCurrentUserIdAsync(HttpClient httpClient)
    {
        var response = await httpClient.GetAsync($"{SpotifyApiUrl}/me");
        response.EnsureSuccessStatusCode();
        var responseStream = await response.Content.ReadAsStreamAsync();
        var user = await JsonSerializer.DeserializeAsync<SpotifyUser>(responseStream);
        return user?.Id;
    }
    
    public static async Task<Playlist?> CreatePlaylistAsync(HttpClient httpClient, string userId, string name, string description, bool isPublic)
    {
        var requestUrl = $"{SpotifyApiUrl}/users/{userId}/playlists";
        var playlistRequest = new CreatePlaylistRequest { Name = name, Description = description, Public = isPublic };
        var jsonContent = JsonSerializer.Serialize(playlistRequest);
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            { Content = new StringContent(jsonContent, Encoding.UTF8, "application/json") };

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) { Console.WriteLine($"API Error Body: {responseBody}"); response.EnsureSuccessStatusCode(); }

        return JsonSerializer.Deserialize<Playlist>(responseBody);
    }
}