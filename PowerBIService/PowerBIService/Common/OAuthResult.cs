using System;
using Newtonsoft.Json;

namespace PowerBIService.Common
{
    public class OAuthResult
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("experies_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("ext_experies_in")]
        public int ExtExpiresIn { get; set; }
        [JsonProperty("experies_on")]
        public int ExpiresOn { get; set; }
        [JsonProperty("not_before")] 
        public int NotBefore { get; set; }
        [JsonProperty("resource")]
        public Uri Resource { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        public string Path { get; set; }
    }
}