using System.Text.Json.Serialization;

namespace mail_provisioner_apply_aliases.Pocos;

public class IdpConfiguration
{
    public string Name { get; set; }
    public string BaseAddress { get; set; }
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; }
    public string Scope { get; set; }
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; }
}
