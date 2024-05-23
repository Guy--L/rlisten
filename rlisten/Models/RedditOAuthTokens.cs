using System;
using System.Text.Json.Serialization;

namespace rlisten.Models
{
    public class RedditOAuthTokens
    {
        private bool dirty = false;

        /*
        [JsonPropertyName("AccessExpiration")]
        public string AccessExpiration { get; set; }
        [JsonPropertyName("RefreshExpiration")]
        public string RefreshExpiration { get; set; }
        */

        [JsonPropertyName("Refresh")]
        public string Refresh { get; set; }
        [JsonPropertyName("Access")]
        public string Access { get; set; }

        [JsonIgnore]
        public string RefreshToken
        {
            get { return Refresh; }
            set
            {
                if (Refresh == value)
                    return;
//              RefreshExpiration = DateTime.UtcNow.AddYears(1).ToString();
                Refresh = value;
                dirty = true;
            }
        }

        [JsonIgnore]
        public string AccessToken
        {
            get { return Access; }
            set
            {
                if (Access == value)
                    return;
//              AccessExpiration = DateTime.UtcNow.AddDays(60).ToString();
                Access = value;
                dirty = true;
            }
        }

        public bool ShouldSerializeAccessToken() => false;
        public bool ShouldSerializeRefreshToken() => false;

        public bool NeedsWrite() => dirty;

        /*
        public bool IsTokenExpired(bool accessToken = false)
        {
            var tokenExpiration = accessToken ? AccessExpiration : RefreshExpiration;
            var refreshExpires = string.IsNullOrEmpty(tokenExpiration) ? DateTime.MinValue : DateTime.Parse(tokenExpiration);
            return DateTime.UtcNow >= refreshExpires;
        }
        */
    }
}

