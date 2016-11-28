using CoreTweet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GrapeN
{
    /// <summary>
    /// NewTweet.xaml の相互作用ロジック
    /// </summary>
    public partial class NewTweet : Window
    {
        public ObservableCollection<TweetMedia> Medias { get; set; }

        public NewTweet()
        {
            InitializeComponent();
            Medias = new ObservableCollection<TweetMedia>();
            DataContext = this;
        }

        public NewTweet(MediaType Type, long TweetId)
        {
            InitializeComponent();
            Medias = new ObservableCollection<TweetMedia>();
            DataContext = this;

            if (MediaList.Visibility == Visibility.Collapsed)
            {
                if (Height < 350) Height = 350;
                MediaList.Visibility = Visibility.Visible;
            }

            switch (Type)
            {
                case MediaType.Quote:
                    Medias.Add(new TweetMedia(MediaType.Quote, TweetId));
                    break;

                case MediaType.Reply:
                    TweetMedia Reply = new TweetMedia(MediaType.Reply, TweetId);
                    Medias.Add(Reply);
                    Text.Text = "@" + Reply.TargetTweet.User.ScreenName + " ";
                    break;
            }

            CountText(null, null);
        }

        void AddImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Dialog = new OpenFileDialog();
            Dialog.FileName = "";
            Dialog.DefaultExt = "*.*";
            Dialog.Filter = "画像ファイル (*.png, *.jpg, *.bmp, *.gif)|*.png;*.jpg;*.bmp;*.gif";
            if (Dialog.ShowDialog() == true)
            {
                if (MediaList.Visibility == Visibility.Collapsed)
                {
                    if (Height < 350) Height = 350;
                    MediaList.Visibility = Visibility.Visible;
                }

                TweetMedia Image = new TweetMedia(MediaType.Image, Dialog.FileName);
                Medias.Add(Image);
            }
        }

        void AddScreen(object sender, RoutedEventArgs e)
        {
            ScreenCapture Capture = new ScreenCapture();
            Capture.ShowDialog();

            if (Capture.Result == true)
            {
                if (MediaList.Visibility == Visibility.Collapsed)
                {
                    if (Height < 350) Height = 350;
                    MediaList.Visibility = Visibility.Visible;
                }

                TweetMedia Image = new TweetMedia(MediaType.Image, Capture.FileName);
                Medias.Add(Image);
                File.Delete(@".\" + Capture.FileName);
            }
        }

        void SendTweet(object sender, RoutedEventArgs e)
        {
            Text.IsEnabled = false;
            TweetButton.IsEnabled = false;

            if (Medias.Count != 0)
            {
                List<long> MediaIds = new List<long>();
                Status QuoteTweet = null;
                Status ReplyTweet = null;

                foreach (TweetMedia Media in Medias)
                {
                    if (Media.Type == MediaType.Image)
                        MediaIds.Add(Media.ImageResult.MediaId);

                    if (Media.Type == MediaType.Quote)
                        QuoteTweet = Media.TargetTweet;

                    if (Media.Type == MediaType.Reply)
                        ReplyTweet = Media.TargetTweet;

                }

                if (QuoteTweet != null)
                {
                    if (MediaIds.Count == 0)
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            status => Text.Text + " https://twitter.com/" + QuoteTweet.User.ScreenName
                                        + "/status/" + QuoteTweet.Id
                        );
                    }
                    else
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            status => Text.Text + " https://twitter.com/" + QuoteTweet.User.ScreenName
                                        + "/status/" + QuoteTweet.Id,
                            media_ids => MediaIds.ToArray()
                        );
                    }
                }
                else if (ReplyTweet != null)
                {
                    if (MediaIds.Count == 0)
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            status => Text.Text,
                            in_reply_to_status_id => ReplyTweet.Id
                        );
                    }
                    else
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            status => Text.Text,
                            in_reply_to_status_id => ReplyTweet.Id,
                            media_ids => MediaIds.ToArray()
                        );
                    }
                }
                else
                {
                    MainWindow.GetInstance().Token.Statuses.Update(
                        status => Text.Text,
                        media_ids => MediaIds.ToArray()
                    );
                }
            }
            else
            {
                MainWindow.GetInstance().Token.Statuses.Update(status => Text.Text);
            }

            Text.IsEnabled = true;
            TweetButton.IsEnabled = false;
            Close();
        }

        void CountText(object sender, TextChangedEventArgs e)
        {
            string Temp = Regex.Replace(
                Text.Text,
                @"s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+",
                "01234567890123"
            ).Replace(Environment.NewLine, "1");

            Count.Content = 140 - Temp.Length - (Medias.Count * 24);
            if ((int)Count.Content < 0)
            {
                Count.Foreground = Brushes.Red;
                TweetButton.IsEnabled = false;
            }
            else
            {
                Count.Foreground = Brushes.Black;
                TweetButton.IsEnabled = true;
            }
        }
    }
}