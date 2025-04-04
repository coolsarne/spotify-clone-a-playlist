using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ClonePlaylist;

public static class Authorize
{
    private static readonly HttpClient HttpClient = new();
    private const string SpotifyAccountsUrl = "https://accounts.spotify.com";
    private static string? _authUrl;

    public static async Task<string> AuthorizeUserAsync(SpotifySettings settings)
    {
        var state = Guid.NewGuid().ToString("N");
        const string scope = "playlist-modify-private playlist-modify-public user-read-private";

        var authUrlBuilder = new StringBuilder($"{SpotifyAccountsUrl}/authorize?");
        authUrlBuilder.Append($"client_id={Uri.EscapeDataString(settings.ClientId!)}&");
        authUrlBuilder.Append($"response_type=code&");
        authUrlBuilder.Append($"redirect_uri={Uri.EscapeDataString(settings.RedirectUri!)}&");
        authUrlBuilder.Append($"scope={Uri.EscapeDataString(scope)}&");
        authUrlBuilder.Append($"state={Uri.EscapeDataString(state)}");
        _authUrl = authUrlBuilder.ToString();

        var authCode = await StartLocalHttpListener(settings.RedirectUri!, state);
        if (string.IsNullOrEmpty(authCode)) return string.Empty;

        return await ExchangeCodeForTokenAsync(settings, authCode);
    }

    private static async Task<string?> StartLocalHttpListener(string redirectUri, string expectedState)
    {
        if (!HttpListener.IsSupported) return null;

        var listener = new HttpListener();
        if (!redirectUri.EndsWith("/")) redirectUri += "/";
        listener.Prefixes.Add(redirectUri);

        try
        {
            listener.Start();
            try
            {
                if (!string.IsNullOrEmpty(_authUrl)) Process.Start(new ProcessStartInfo(_authUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open browser: {ex.Message}");
            }

            var context = await listener.GetContextAsync();
            var request = context.Request;
            var code = request.QueryString["code"];
            var receivedState = request.QueryString["state"];
            var error = request.QueryString["error"];

            const string responseString = "<html><body><h1>Authorization Received</h1><p>Return to console.</p></body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            HttpListenerResponse response = context.Response;
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Auth error: {error}");
                return null;
            }

            if (string.IsNullOrEmpty(receivedState) || receivedState != expectedState)
            {
                Console.WriteLine("State mismatch!");
                return null;
            }

            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("Code missing!");
                return null;
            }

            return code;
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 5)
        {
            Console.WriteLine("Access denied listening...");
            return null;
        }
        catch (SocketException sockEx) when (sockEx.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            Console.WriteLine("Port in use...");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Listener error: {ex.Message}");
            return null;
        }
        finally
        {
            if (listener.IsListening)
            {
                listener.Stop();
                listener.Close();
            }
        }
    }

    private static async Task<string> ExchangeCodeForTokenAsync(SpotifySettings settings, string code)
    {
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{SpotifyAccountsUrl}/api/token");
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{settings.ClientId}:{settings.ClientSecret}"));
        tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", settings.RedirectUri! }
        });

        var response = await HttpClient.SendAsync(tokenRequest);
        response.EnsureSuccessStatusCode();
        var responseStream = await response.Content.ReadAsStreamAsync();
        var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(responseStream);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken)) throw new InvalidOperationException("Bad token response");

        return tokenResponse.AccessToken;
    }
}