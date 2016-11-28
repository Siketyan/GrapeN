using Newtonsoft.Json;
using System.ComponentModel;

namespace GrapeN.Config
{
    [JsonObject("config")]
    public class Config
    {
        [JsonProperty("color")]
        public Color ColorConfig { get; set; }

        [JsonProperty("key")]
        public Key KeyConfig { get; set; }
    }

    [JsonObject("color")]
    public class Color
    {
        [JsonProperty("background")]
        [DefaultValue("#FF272727")]
        public string Background { get; set; }

        [JsonProperty("tab")]
        [DefaultValue("#FFFFFFFF")]
        public string Tab { get; set; }

        [JsonProperty("tab_active")]
        [DefaultValue("#FF3399FF")]
        public string TabActive { get; set; }

        [JsonProperty("tweet_username")]
        [DefaultValue("White")]
        public string TweetUsername { get; set; }

        [JsonProperty("tweet_id")]
        [DefaultValue("#FF6E6E6E")]
        public string TweetId { get; set; }

        [JsonProperty("tweet_via")]
        [DefaultValue("#FF6E6E6E")]
        public string TweetVia { get; set; }

        [JsonProperty("tweet_body")]
        [DefaultValue("White")]
        public string TweetBody { get; set; }

        [JsonProperty("tweet_retweet")]
        [DefaultValue("LightGreen")]
        public string TweetRetweet { get; set; }

        [JsonProperty("tweet_like")]
        [DefaultValue("Red")]
        public string TweetLike { get; set; }

        [JsonProperty("tweet_action")]
        [DefaultValue("Gray")]
        public string TweetAction { get; set; }
    }

    [JsonObject("key")]
    public class Key
    {
        [JsonProperty("like")]
        [DefaultValue("L")]
        public string Like { get; set; }

        [JsonProperty("retweet")]
        [DefaultValue("R")]
        public string Retweet { get; set; }

        [JsonProperty("new")]
        [DefaultValue("N")]
        public string NewTweet { get; set; }

        [JsonProperty("home")]
        [DefaultValue("H")]
        public string Home { get; set; }

        [JsonProperty("notifications")]
        [DefaultValue("M")]
        public string Notifications { get; set; }
    }
}
