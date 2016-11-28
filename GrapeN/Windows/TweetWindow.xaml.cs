using CoreTweet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace GrapeN.Windows
{
    /// <summary>
    /// NewTweet.xaml の相互作用ロジック
    /// </summary>
    public partial class TweetWindow
    {
        private ObservableCollection<TweetMedia> Medias { get; }

        public TweetWindow()
        {
            InitializeComponent();
            Medias = new ObservableCollection<TweetMedia>();
            MediaList.ItemsSource = Medias;
            DataContext = this;
        }

        public TweetWindow(MediaType type, long tweetId)
        {
            InitializeComponent();
            Medias = new ObservableCollection<TweetMedia>();
            MediaList.ItemsSource = Medias;
            DataContext = this;

            if (MediaList.Visibility == Visibility.Collapsed)
            {
                if (Height < 350) Height = 350;
                MediaList.Visibility = Visibility.Visible;
            }

            switch (type)
            {
                case MediaType.Quote:
                    Medias.Add(new TweetMedia(MediaType.Quote, tweetId));
                    break;

                case MediaType.Reply:
                    var reply = new TweetMedia(MediaType.Reply, tweetId);
                    Medias.Add(reply);
                    Text.Text = "@" + reply.TargetTweet.User.ScreenName + " ";
                    break;
                
                case MediaType.Image:
                case MediaType.Video:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            CountText(null, null);
        }

        private void Init(object sender, RoutedEventArgs e)
        {
            Text.Focusable = true;
            Text.Focus();
        }

        private void AddImage(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                FileName = "",
                DefaultExt = "*.*",
                Filter = "画像ファイル (*.png, *.jpg, *.bmp, *.gif)|*.png;*.jpg;*.bmp;*.gif"
            };

            if (dialog.ShowDialog() != true) return;
            if (MediaList.Visibility == Visibility.Collapsed)
            {
                if (Height < 350) Height = 350;
                MediaList.Visibility = Visibility.Visible;
            }

            var image = new TweetMedia(MediaType.Image, dialog.FileName);
            Medias.Add(image);
        }

        private void AddScreen(object sender, RoutedEventArgs e)
        {
            var capture = new ScreenCapture();
            capture.ShowDialog();

            if (capture.Result != true) return;
            if (MediaList.Visibility == Visibility.Collapsed)
            {
                if (Height < 350) Height = 350;
                MediaList.Visibility = Visibility.Visible;
            }

            var image = new TweetMedia(MediaType.Image, capture.FileName);
            Medias.Add(image);
            File.Delete(@".\" + capture.FileName);
        }

        private void SendTweet(object sender, RoutedEventArgs e)
        {
            Text.IsEnabled = false;
            TweetButton.IsEnabled = false;

            if (Medias.Count != 0)
            {
                var mediaIds = new List<long>();
                Status quoteTweet = null;
                Status replyTweet = null;

                foreach (var media in Medias)
                {
                    if (media.Type == MediaType.Image)
                        mediaIds.Add(media.ImageResult.MediaId);

                    if (media.Type == MediaType.Quote)
                        quoteTweet = media.TargetTweet;

                    if (media.Type == MediaType.Reply)
                        replyTweet = media.TargetTweet;

                }

                if (quoteTweet != null)
                {
                    if (mediaIds.Count == 0)
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            Text.Text + " https://twitter.com/" + quoteTweet.User.ScreenName
                                        + "/status/" + quoteTweet.Id
                        );
                    }
                    else
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            Text.Text + " https://twitter.com/" + quoteTweet.User.ScreenName
                                        + "/status/" + quoteTweet.Id,
                            media_ids: mediaIds.ToArray()
                        );
                    }
                }
                else if (replyTweet != null)
                {
                    if (mediaIds.Count == 0)
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            Text.Text, replyTweet.Id
                        );
                    }
                    else
                    {
                        MainWindow.GetInstance().Token.Statuses.Update(
                            Text.Text, replyTweet.Id,
                            media_ids: mediaIds.ToArray()
                        );
                    }
                }
                else
                {
                    MainWindow.GetInstance().Token.Statuses.Update(
                        Text.Text,
                        media_ids: mediaIds.ToArray()
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

        private void CountText(object sender, TextChangedEventArgs e)
        {
            var temp = Regex.Replace(
                Text.Text,
                @"s?https?://[-_.!~*'()a-zA-Z0-9;/?:@&=+$,%#]+",
                "01234567890123"
            ).Replace(Environment.NewLine, "1");

            Count.Content = 140 - temp.Length - (Medias.Count * 24);
            if ((int)Count.Content < 0)
            {
                Count.Foreground = Brushes.Red;
                TweetButton.IsEnabled = false;
            }
            else
            {
                Count.Foreground = Brushes.White;
                TweetButton.IsEnabled = true;
            }
        }

        private void ParseKeyboardShortcut(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.None) return;

            e.Handled = true;
            SendTweet(null, null);
        }
    }
}