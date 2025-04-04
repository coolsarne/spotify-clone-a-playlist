using System.Text.Json.Serialization;

namespace ClonePlaylist;

public record SpotifySettings
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUri { get; init; }
}

public record TokenResponse
{
    [JsonPropertyName("access_token")] public required string AccessToken { get; init; }
    [JsonPropertyName("token_type")] public required string TokenType { get; init; }
    [JsonPropertyName("scope")] public required string Scope { get; init; }
    [JsonPropertyName("expires_in")] public required int ExpiresIn { get; init; }
    [JsonPropertyName("refresh_token")] public required string? RefreshToken { get; init; }
}

public record SpotifyUser
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("display_name")] public required string? DisplayName { get; init; }
}

public record Playlist
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("description")] public required string? Description { get; init; }
    [JsonPropertyName("external_urls")] public required Dictionary<string, string>? ExternalUrls { get; init; }
}

public record CreatePlaylistRequest
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("public")] public required bool Public { get; init; }
    [JsonPropertyName("description")] public required string Description { get; init; }
}