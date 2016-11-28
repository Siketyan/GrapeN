using CoreTweet;
using CoreTweet.Streaming;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrapeN
{
    public class Tweet
    {
        MainWindow Instance;

        /* 色設定 */
        public string ColorRetweet { get; set; }
        public string ColorLike { get; set; }

        /* ツイート固有 */
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

        /* 引用リツイート */
        public string QuoteVisible { get; set; }
        public string QuoteTweetId { get; set; }
        public string QuoteName { get; set; }
        public string QuoteId { get; set; }
        public string QuoteVia { get; set; }
        public string QuoteText { get; set; }
        public string QuoteColor { get; set; }
        public string QuoteImage { get; set; }

        public Tweet(Status Msg)
        {
            Instance = MainWindow.GetInstance();
            
            ColorRetweet = "Gray";
            ColorLike = "Gray";

            QuoteVisible = "Collapsed";
            TweetId = Msg.Id.ToString();
            Name = Msg.User.Name;
            Id = " @" + Msg.User.ScreenName;
            Via = FormatVia(Msg.Source);
            Text = Msg.Text;
            Color = "#442523";
            Image = Msg.User.ProfileImageUrlHttps;
            RtBack = "Transperent";
            RtSep = "Trapsperent";

            if ((bool)Msg.IsRetweeted) ColorRetweet = "LightGreen";
            if ((bool)Msg.IsFavorited) ColorLike = "Red";

            Status Quote = Msg.QuotedStatus;
            if (Quote != null)
            {
                QuoteVisible = "Visible";
                QuoteTweetId = Quote.Id.ToString();
                QuoteName = Quote.User.Name;
                QuoteId = " @" + Quote.User.ScreenName;
                QuoteVia = FormatVia(Quote.Source);
                QuoteText = Quote.Text;
                QuoteColor = Color;
                QuoteImage = Quote.User.ProfileImageUrlHttps;
            }

            Status Stats = Msg.RetweetedStatus;
            if (Stats != null)
            {
                TweetId = Stats.Id.ToString();
                Name = Stats.User.Name + " (" + Msg.User.Name + " Retweeted)";
                Id = " @" + Stats.User.ScreenName + " (RT: @" + Msg.User.ScreenName + ")";
                Via = FormatVia(Stats.Source);
                Text = Stats.Text;
                Color = "Green";
                Image = Stats.User.ProfileImageUrlHttps;
                RtImage = Msg.User.ProfileImageUrlHttps;
                RtBack = "White";
                RtSep = "White";

                if ((bool)Stats.IsRetweeted) ColorRetweet = "LightGreen";
                if ((bool)Stats.IsFavorited) ColorLike = "Red";
            }
        }

        public string FormatVia(string Via)
        {
            Regex rg = new Regex(@"^<(.+)>(?<Via>.+?)</a>$");
            Match m = rg.Match(Via);
            return "  [via " + m.Groups["Via"].Value + "]";
        }

        BitmapImage GetImage(string Url)
        {
            var image = new BitmapImage();
            int BytesToRead = 100;

            WebRequest request = WebRequest.Create(new Uri(Url));
            request.Timeout = -1;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            BinaryReader reader = new BinaryReader(responseStream);
            MemoryStream memoryStream = new MemoryStream();

            byte[] bytebuffer = new byte[BytesToRead];
            int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

            while (bytesRead > 0)
            {
                memoryStream.Write(bytebuffer, 0, bytesRead);
                bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
            }

            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);

            image.StreamSource = memoryStream;
            image.EndInit();

            return image;
        }
    }
}
