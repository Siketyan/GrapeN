using System.ComponentModel;
using CoreTweet;
using System.Text.RegularExpressions;

namespace GrapeN
{
    public class Tweet : INotifyPropertyChanged
    {
        /* 共通設定(Config) */
        public string ColorId { get; set; }
        public string ColorVia { get; set; }
        public string ColorBody { get; set; }

        private string _colorRetweet;
        public string ColorRetweet
        {
            get { return _colorRetweet; }
            set
            {
                _colorRetweet = value;
                OnPropertyChanged("ColorRetweet");
            }
        }

        private string _colorLike;
        public string ColorLike
        {
            get { return _colorLike; }
            set
            {
                _colorLike = value;
                OnPropertyChanged("ColorLike");
            }
        }

        /* ツイート固有 */
        public string UserId { get; set; }
        public string TweetId { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Via { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
        public string Image { get; set; }
        public string RtBack { get; set; }
        public string RtSep { get; set; }
        public string RtImage { get; set; }
        public string RtUser { get; set; }

        /* 引用リツイート */
        public string QuoteVisible { get; set; }
        public string QuoteTweetId { get; set; }
        public string QuoteName { get; set; }
        public string QuoteId { get; set; }
        public string QuoteVia { get; set; }
        public string QuoteText { get; set; }
        public string QuoteColor { get; set; }
        public string QuoteImage { get; set; }
        public string QuoteUser { get; set; }

        private readonly MainWindow _instance;

        public Tweet(Status msg)
        {
            _instance = MainWindow.GetInstance();

            ColorId = _instance.Conf.ColorConfig.TweetId;
            ColorVia = _instance.Conf.ColorConfig.TweetVia;
            ColorBody = _instance.Conf.ColorConfig.TweetBody;
            ColorRetweet = _instance.Conf.ColorConfig.TweetAction;
            ColorLike = _instance.Conf.ColorConfig.TweetAction;

            QuoteVisible = "Collapsed";
            UserId = msg.User.Id.ToString();
            TweetId = msg.Id.ToString();
            Name = msg.User.Name;
            Id = " @" + msg.User.ScreenName;
            Via = FormatVia(msg.Source);
            Text = msg.Text;
            Color = _instance.Conf.ColorConfig.TweetUsername;
            Image = msg.User.ProfileImageUrlHttps;
            RtBack = "Transperent";
            RtSep = "Trapsperent";

            if (msg.IsRetweeted != null && (bool)msg.IsRetweeted)
                ColorRetweet = _instance.Conf.ColorConfig.TweetRetweet;
            if (msg.IsFavorited != null && (bool)msg.IsFavorited)
                ColorLike = _instance.Conf.ColorConfig.TweetLike;

            var quote = msg.QuotedStatus;
            if (quote != null)
            {
                QuoteVisible = "Visible";
                QuoteTweetId = quote.Id.ToString();
                QuoteName = quote.User.Name;
                QuoteId = " @" + quote.User.ScreenName;
                QuoteVia = FormatVia(quote.Source);
                QuoteText = quote.Text;
                QuoteColor = Color;
                QuoteImage = quote.User.ProfileImageUrlHttps;
                QuoteUser = quote.User.Id.ToString();
            }

            var stats = msg.RetweetedStatus;
            if (stats == null) return;

            TweetId = stats.Id.ToString();
            UserId = stats.User.Id.ToString();
            Name = stats.User.Name + " (" + msg.User.Name + " Retweeted)";
            Id = " @" + stats.User.ScreenName + " (RT: @" + msg.User.ScreenName + ")";
            Via = FormatVia(stats.Source);
            Text = stats.Text;
            Color = "LightGreen";
            Image = stats.User.ProfileImageUrlHttps;
            RtImage = msg.User.ProfileImageUrlHttps;
            RtBack = "White";
            RtSep = "White";
            RtUser = msg.User.Id.ToString();

            if (stats.IsRetweeted != null && (bool)stats.IsRetweeted)
                ColorRetweet = _instance.Conf.ColorConfig.TweetRetweet;
            if (stats.IsFavorited != null && (bool)stats.IsFavorited)
                ColorLike = _instance.Conf.ColorConfig.TweetLike;
        }

        public Tweet(Status msg, User source) : this(msg)
        {
            Color = "Pink";
            Name = msg.User.Name + " (" + source.Name + " Liked)";
            RtImage = source.ProfileImageUrlHttps;
            RtBack = "White";
            RtSep = "White";
            RtUser = source.Id.ToString();
            
            if (msg.IsRetweeted != null && (bool)msg.IsRetweeted)
                ColorRetweet = _instance.Conf.ColorConfig.TweetRetweet;
            if (msg.IsFavorited != null && (bool)msg.IsFavorited)
                ColorLike = _instance.Conf.ColorConfig.TweetLike;
        }

        private static string FormatVia(string via)
        {
            var rg = new Regex(@"^<(.+)>(?<Via>.+?)</a>$");
            var m = rg.Match(via);
            return "  [via " + m.Groups["Via"].Value + "]";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
