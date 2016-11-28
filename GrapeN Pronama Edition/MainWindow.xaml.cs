using CoreTweet;
using CoreTweet.Streaming;
using GrapeN.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrapeN
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public DispatcherCollection<Tweet> Tweets { get; set; }
        public DispatcherCollection<Tweet> Mentions { get; set; }

        static MainWindow Instance;

        public Tokens Token;
        public Settings Setting;
        SettingsLoader Loader;
        User Me;
        System.Windows.Forms.NotifyIcon Notifier;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            Tweets = new DispatcherCollection<Tweet>();
            Mentions = new DispatcherCollection<Tweet>();
            
            DataContext = this;
        }

        async void Init(object sender, RoutedEventArgs e)
        {
            Notifier = new System.Windows.Forms.NotifyIcon();
            Notifier.Text = "GrapeN Notifier (Pronama Edition)";
            Notifier.Icon = new System.Drawing.Icon(
                                Application.GetResourceStream(
                                    new Uri("pack://application:,,,/GrapeN Pronama Edition;component/grapen.ico")
                                ).Stream
                            );
            //Notifier.Visible = true;
            Notifier.BalloonTipTitle = "GrapeN";
            Notifier.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;

            if (!File.Exists(@".\settings.grape"))
                new Auth().ShowDialog();

            if (!File.Exists(@".\settings.grape"))
                Environment.Exit(0);

            Loader = new SettingsLoader(@".\settings.grape");
            Setting = Loader.Setting;
            Loader.Dispose();
            Token = Tokens.Create(
                __Private.ConsumerKey, //ConsumerKey
                __Private.ConsumerSecret, //ConsumerSecret
                Setting.AccessToken, //AccessToken
                Setting.AccessTokenSecret //AccessTokenSecret
            );

            if (Setting.AutoActions == null || !(Setting.AutoActions is ObservableCollection<AutoAction>))
            {
                Setting.AutoActions = new ObservableCollection<AutoAction>();
                //Setting.AutoActions.Add(new AutoAction(ActionType.Like, true, "TestPattern", "TestMessage"));
                SettingsWriter.Write(Setting, Loader.Path);
            }

            //new AutoActionsWindow().ShowDialog();

            Me = Token.Users.Show(user_id => Token.AccessToken.Split('-')[0]);
            MyImage.Source = new BitmapImage(new Uri(Me.ProfileImageUrlHttps));
            MyName.Content = Me.Name;
            
            foreach(Status AStatus in await Token.Statuses.HomeTimelineAsync(count => 50))
            {
                Tweets.Insert(0, new Tweet(AStatus));
            }

            foreach (Status AMentionStatus in await Token.Statuses.MentionsTimelineAsync(count => 50))
            {
                Mentions.Insert(0, new Tweet(AMentionStatus));
            }
            

            var Stream = Token.Streaming.UserAsObservable().Publish();

            Stream.OfType<EventMessage>().Subscribe(x =>
                {
                    if (x.Target.Id != Me.Id) return;
                    if (x.Event == EventCode.AccessRevoked || x.Event == EventCode.Block
                        || x.Event == EventCode.ListCreated || x.Event == EventCode.ListDestroyed
                        || x.Event == EventCode.ListMemberRemoved || x.Event == EventCode.ListUpdated
                        || x.Event == EventCode.ListUserSubscribed || x.Event == EventCode.ListUserUnsubscribed
                        || x.Event == EventCode.Mute || x.Event == EventCode.Unblock
                        || x.Event == EventCode.Unfavorite || x.Event == EventCode.Unfollow
                        || x.Event == EventCode.Unmute || x.Event == EventCode.UserUpdate) return;
                    
                    if (x.Event == EventCode.Favorite)
                        Notifier.BalloonTipText = x.Source.Name + " (@" + x.Source.ScreenName + ") さんがツイートをいいねしました\n\n"
                            + x.TargetStatus.Text;
                    if (x.Event == EventCode.FavoritedRetweet)
                        Notifier.BalloonTipText = x.Source.Name + " (@" + x.Source.ScreenName + ") さんがリツイートをいいねしました\n\n@"
                            + x.TargetStatus.Text;
                    if (x.Event == EventCode.Follow)
                        Notifier.BalloonTipText = x.Source.Name + " (@" + x.Source.ScreenName + ") さんからフォローされました";
                    if (   x.Event == EventCode.AccessRevoked      || x.Event == EventCode.Block
                        || x.Event == EventCode.ListCreated        || x.Event == EventCode.ListDestroyed
                        || x.Event == EventCode.ListMemberRemoved  || x.Event == EventCode.ListUpdated
                        || x.Event == EventCode.ListUserSubscribed || x.Event == EventCode.ListUserUnsubscribed
                        || x.Event == EventCode.Mute               || x.Event == EventCode.Unblock
                        || x.Event == EventCode.Unfavorite         || x.Event == EventCode.Unfollow
                        || x.Event == EventCode.Unmute             || x.Event == EventCode.UserUpdate) return;
                    if (x.Event == EventCode.ListMemberAdded)
                        Notifier.BalloonTipText = x.Source.Name + " (@" + x.Source.ScreenName + ") さんのリスト "
                            + x.TargetList.Name + " に追加されました";
                    if (x.Event == EventCode.QuotedTweet)
                        Notifier.BalloonTipText = x.Source.Name + " (@" + x.Source.ScreenName + ") さんがツイートを引用リツイートしました\n\n"
                            + x.TargetStatus.Text + "\nQT " + x.TargetStatus.QuotedStatus.Text;
                    if (x.Event == EventCode.RetweetedRetweet)
                        Notifier.BalloonTipText = x.Source.Name + " (@" + x.Source.ScreenName + ") さんがリツイートをリツイートしました\n\n"
                            + x.TargetStatus.Text;
                    
                    Notifier.ShowBalloonTip(3000);
                });

            Stream.OfType<StatusMessage>().Subscribe(x =>
                {
                    if (x.Status.InReplyToUserId == Me.Id)
                    {
                        Mentions.RemoveAt(0);
                        Mentions.Add(new Tweet(x.Status));

                        Notifier.BalloonTipText = x.Status.User.Name + " (@" + x.Status.User.ScreenName
                            + ") さんからの返信\n\n" + x.Status.Text;
                        Notifier.ShowBalloonTip(3000);
                    }
                    if (x.Status.RetweetedStatus != null && x.Status.RetweetedStatus.User.Id == Me.Id)
                    {
                        Notifier.BalloonTipText = x.Status.User.Name + " (@" + x.Status.User.ScreenName
                            + ") さんがツイートをリツイートしました\n\n" + x.Status.RetweetedStatus.Text;
                        Notifier.ShowBalloonTip(3000);
                    }

                    Tweets.RemoveAt(0);
                    Tweets.Add(new Tweet(x.Status));
                });

            var disposable = Stream.Connect();
        }

        void OpenNewTweet(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowOrActivate<NewTweet>();
        }

        void OpenSettings(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowOrActivate<AutoActionsWindow>();
        }

        void Retweet(object sender, RoutedEventArgs e)
        {
            if (((SolidColorBrush)((Button)sender).Foreground).Color != Brushes.Gray.Color)
                return;


            ((Button)sender).Foreground = Brushes.LightGreen;
            Token.Statuses.RetweetAsync(id => long.Parse(((Button)sender).Tag.ToString()));
        }

        async void Like(object sender, RoutedEventArgs e)
        {
            long Id = long.Parse(((Button)sender).Tag.ToString());

            if (((SolidColorBrush)((Button)sender).Foreground).Color != Brushes.Gray.Color)
            {
                ((Button)sender).Foreground = Brushes.Gray;
                await Token.Favorites.DestroyAsync(id => Id);
                return;
            }


            ((Button)sender).Foreground = Brushes.Red;
            await Token.Favorites.CreateAsync(id => Id);
        }

        void QuoteTweet(object sender, RoutedEventArgs e)
        {
            new NewTweet(MediaType.Quote, long.Parse((string)((MenuItem)sender).Tag)).Show();
        }

        void ReplyTweet(object sender, RoutedEventArgs e)
        {
            new NewTweet(MediaType.Reply, long.Parse((string)((MenuItem)sender).Tag)).Show();
        }

        void SwitchTab(object sender, RoutedEventArgs e)
        {
            if ((string)((Button)sender).Tag == "Home")
            {
                TabMentions.BorderBrush = null;
                TabHome.BorderBrush
                    = new SolidColorBrush(StringToColor("#FF3399FF"));
                MentionsTimeline.Visibility = Visibility.Collapsed;
                Timeline.Visibility = Visibility.Visible;
            }
            else
            {
                TabHome.BorderBrush = null;
                TabMentions.BorderBrush
                    = new SolidColorBrush(StringToColor("#FF3399FF"));
                Timeline.Visibility = Visibility.Collapsed;
                MentionsTimeline.Visibility = Visibility.Visible;
            }
        }

        Color StringToColor(string Str)
        {
            return (Color)ColorConverter.ConvertFromString(Str);
        }

        public static MainWindow GetInstance()
        {
            return Instance;
        }
    }
}
