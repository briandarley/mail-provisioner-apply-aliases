

using mail_provisioner_apply_aliases.Pocos;

public interface ITokenService
{
    Task<string> GetTokenAsync();
}


public class TokenService : ITokenService
{
    private DateTime _tokenExpiry = DateTime.MinValue;
    private string _token = string.Empty;
    private string _identityServer;
    private readonly List<IdpConfiguration> _idpConfigurations;

    public TokenService(string identityServer, List<IdpConfiguration> idpConfigurations)
    {
        _identityServer = identityServer;
        _idpConfigurations = idpConfigurations;
    }
    public async Task<string> GetTokenAsync()
    {

        // Check if the token is expired or about to expire
        if (DateTime.UtcNow >= _tokenExpiry)
        {
            // Logic to fetch a new token
            // For example purposes, let's assume we're calling some API to get the token
            var accessToken = await FetchNewTokenAsync();
            _token = accessToken.access_token;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(20); // Set the expiry time
        }

        return _token;
    }



    private async Task<AccessToken> FetchNewTokenAsync()
    {
        var idpConfiguration = _idpConfigurations.FirstOrDefault(x => x.Name == _identityServer);

        var tokenRequest = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, $"{idpConfiguration.BaseAddress}/connect/token")
        {
            Content = new System.Net.Http.FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = idpConfiguration.ClientId,
                ["client_secret"] = idpConfiguration.ClientSecret,
                ["grant_type"] = idpConfiguration.GrantType,
                ["scope"] = idpConfiguration.Scope
            })
        };

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;


        using var client = new System.Net.Http.HttpClient(handler);
        var tokenResponse = await client.SendAsync(tokenRequest, HttpCompletionOption.ResponseHeadersRead);
        tokenResponse.EnsureSuccessStatusCode();

        var body = await tokenResponse.Content.ReadAsStringAsync();

        var token = System.Text.Json.JsonSerializer.Deserialize<AccessToken>(body, JsonOptions.Options);

        if (token == null)
        {
            throw new Exception("Failed to get token");
        }
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token.access_token);

        token.EmpireDateTime = jwt.ValidTo.ToLocalTime();



        return token;
    }
}