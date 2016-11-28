using CoreTweet;
using System;
using System.IO;
using System.Windows;

namespace GrapeN
{
    public class TweetMedia
    {
        public string Name { get; set; }
        public string Icon { get; set; }

        /* 共通 */
        public MediaType Type;

        /* Image */
        public string FullUrl;
        public MediaUploadResult ImageResult;

        /* Quote & Reply */
        public Status TargetTweet;

        public TweetMedia(MediaType Type, params object[] Data)
        {
            this.Type = Type;

            switch (Type)
            {
                case MediaType.Image: // [0] => (string)Url
                    Name = Path.GetFileName((string)Data[0]);
                    Icon = "";
                    FullUrl = (string)Data[0];
                    ImageResult = MainWindow.GetInstance().Token.Media.Upload(media => new FileInfo(FullUrl));
                    break;

                case MediaType.Quote: // [0] => (long)TweetId
                    TargetTweet = MainWindow.GetInstance().Token.Statuses.Show(id => (long)Data[0]);
                    Name = "@" + TargetTweet.User.ScreenName + ": " + TargetTweet.Text.Replace(Environment.NewLine, "");
                    Icon = "";
                    break;

                case MediaType.Reply: // [0] => (long)TweetId
                    TargetTweet = MainWindow.GetInstance().Token.Statuses.Show(id => (long)Data[0]);
                    Name = "@" + TargetTweet.User.ScreenName + ": " + TargetTweet.Text.Replace(Environment.NewLine, "");
                    Icon = "";
                    break;
            }
        }
    }

    public enum MediaType
    {
        Image,
        Video,
        Quote,
        Reply
    }
}