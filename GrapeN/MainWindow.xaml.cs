using CoreTweet;
using CoreTweet.Streaming;
using GrapeN.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Forms = System.Windows.Forms;

namespace GrapeN
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        private const int WM_SIZE = 0x0005;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;

        public Config.Config Conf { get; }
        private DispatcherCollection<Tweet> Tweets { get; }
        private DispatcherCollection<Tweet> Notifications { get; }

        public Tokens Token;
        public Settings Setting;

        private static MainWindow _instance;
        private bool _isSizing;
        private SettingsLoader _loader;
        private User _me;
        private Forms.NotifyIcon _notifier;
        private IntPtr lastLParam;
        private IntPtr lastWParam;

        public MainWindow()
        {
            _instance = this;
            InitializeComponent();

            string json;
            if (File.Exists(@".\config.json"))
            {
                using (var sr = new StreamReader(
                            @".\config.json", Encoding.GetEncoding("Shift_JIS")))
                {
                    json = sr.ReadToEnd();
                }
            }
            else json = "{\"color\":{}, \"key\":{}}";

            Conf = JsonConvert.DeserializeObject<Config.Config>(json,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });
            Tweets = new DispatcherCollection<Tweet>();
            Notifications = new DispatcherCollection<Tweet>();

            Timeline.ItemsSource = Tweets;
            NotificationsTimeline.ItemsSource = Notifications;

            DataContext = this;
        }

        private async void Init(object sender, RoutedEventArgs e)
        {
            var hsrc = HwndSource.FromVisual(this) as HwndSource;
            hsrc.AddHook(WndProc);

            var iconStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/GrapeN;component/grapen.ico")
            );

            if (iconStream != null)
                _notifier = new Forms.NotifyIcon
                {
                    Text = @"GrapeN Notifier",
                    Icon = new System.Drawing.Icon(
                        iconStream.Stream
                    ),
                    Visible = true,
                    BalloonTipTitle = @"GrapeN",
                    BalloonTipIcon = Forms.ToolTipIcon.Info
                };

            if (!File.Exists(@".\settings.grape"))
                new AuthWindow().ShowDialog();

            if (!File.Exists(@".\settings.grape"))
                Environment.Exit(0);

            _loader = new SettingsLoader(@".\settings.grape");
            Setting = _loader.Setting;
            _loader.Dispose();
            Token = Tokens.Create(
                __Private.ConsumerKey, //ConsumerKey
                __Private.ConsumerSecret, //ConsumerSecret
                Setting.AccessToken, //AccessToken
                Setting.AccessTokenSecret //AccessTokenSecret
            );

            if (Setting.AutoActions == null)
            {
                Setting.AutoActions = new ObservableCollection<AutoAction>();
                SettingsWriter.Write(Setting, _loader.Path);
            }

            _me = Token.Users.Show(long.Parse(Token.AccessToken.Split('-')[0]));
            MyImage.Source = new BitmapImage(new Uri(_me.ProfileImageUrlHttps));
            MyName.Content = _me.Name;
            
            foreach(var status in await Token.Statuses.HomeTimelineAsync(50))
            {
                Tweets.Insert(0, new Tweet(status));
            }

            foreach (var status in await Token.Statuses.MentionsTimelineAsync(50))
            {
                Notifications.Insert(0, new Tweet(status));
            }
            

            var stream = Token.Streaming.UserAsObservable().Publish();

            stream.OfType<EventMessage>().Subscribe(x =>
                {
                    if (x.Source.Id == _me.Id)
                    {
                        switch (x.Event)
                        {
                            case EventCode.Favorite:
                                foreach (var t in Tweets)
                                {
                                    if (t.TweetId != x.TargetStatus.Id.ToString()) continue;
                                    if (t.ColorLike != Conf.ColorConfig.TweetAction) break;

                                    t.ColorLike = Conf.ColorConfig.TweetLike;
                                    break;
                                }

                                foreach (var t in Notifications)
                                {
                                    if (t.TweetId != x.TargetStatus.Id.ToString()) continue;
                                    if (t.ColorLike != Conf.ColorConfig.TweetAction) break;

                                    t.ColorLike = Conf.ColorConfig.TweetLike;
                                    break;
                                }
                                break;

                            case EventCode.Unfavorite:
                                foreach (var t in Tweets)
                                {
                                    if (t.TweetId != x.TargetStatus.Id.ToString()) continue;
                                    if (t.ColorLike == Conf.ColorConfig.TweetAction) break;

                                    t.ColorLike = Conf.ColorConfig.TweetAction;
                                    break;
                                }

                                foreach (var t in Notifications)
                                {
                                    if (t.TweetId != x.TargetStatus.Id.ToString()) continue;
                                    if (t.ColorLike == Conf.ColorConfig.TweetAction) break;

                                    t.ColorLike = Conf.ColorConfig.TweetAction;
                                    break;
                                }
                                break;
                        }
                    }

                    if (x.Target.Id != _me.Id) return;
                    switch (x.Event)
                    {
                        case EventCode.AccessRevoked:
                        case EventCode.Block:
                        case EventCode.ListCreated:
                        case EventCode.ListDestroyed:
                        case EventCode.ListMemberRemoved:
                        case EventCode.ListUpdated:
                        case EventCode.ListUserSubscribed:
                        case EventCode.ListUserUnsubscribed:
                        case EventCode.Mute:
                        case EventCode.Unblock:
                        case EventCode.Unfavorite:
                        case EventCode.Unfollow:
                        case EventCode.Unmute:
                        case EventCode.UserUpdate:
                            return;
                        
                        case EventCode.Favorite:
                            _notifier.BalloonTipText =
                                x.Source.Name + @" (@" + x.Source.ScreenName
                                    + ") さんがツイートをいいねしました\n\n" + x.TargetStatus.Text;
                            Notifications.Add(new Tweet(x.TargetStatus, x.Source));
                            break;
                        
                        case EventCode.FavoritedRetweet:
                            _notifier.BalloonTipText =
                                x.Source.Name + @" (@" + x.Source.ScreenName
                                    + ") さんがリツイートをいいねしました\n\n@" + x.TargetStatus.Text;
                            Notifications.Add(new Tweet(x.TargetStatus.RetweetedStatus, x.Source));
                            break;
                        
                        case EventCode.Follow:
                            _notifier.BalloonTipText =
                                x.Source.Name + @" (@" + x.Source.ScreenName + @") さんからフォローされました";
                            break;
                        
                        case EventCode.ListMemberAdded:
                            _notifier.BalloonTipText =
                                x.Source.Name + @" (@" + x.Source.ScreenName
                                    + @") さんのリスト " + x.TargetList.Name + @" に追加されました";
                            break;

                        case EventCode.QuotedTweet:
                            _notifier.BalloonTipText =
                                x.Source.Name + @" (@" + x.Source.ScreenName
                                    + ") さんがツイートを引用リツイートしました\n\n"
                                    + x.TargetStatus.Text + "\nQT " + x.TargetStatus.QuotedStatus.Text;
                            Notifications.Add(new Tweet(x.TargetStatus));
                            break;
                        
                        case EventCode.RetweetedRetweet:
                            _notifier.BalloonTipText =
                                x.Source.Name + @" (@" + x.Source.ScreenName
                                    + ") さんがリツイートをリツイートしました\n\n" + x.TargetStatus.Text;
                            Notifications.Add(new Tweet(x.TargetStatus));
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    
                    _notifier.ShowBalloonTip(3000);
                });

            stream.OfType<StatusMessage>().Subscribe(x =>
                {
                    if (x.Status.RetweetedStatus != null && x.Status.User.Id == x.Status.RetweetedStatus.User.Id) return;
                    
                    foreach (var action in Setting.AutoActions)
                    {
                        var isMatch = (action.UseRegex)
                            ? Regex.IsMatch(x.Status.Text, action.Pattern)
                            : x.Status.Text.Contains(action.Pattern);
                        if (!isMatch) continue;

                        switch (action.Type)
                        {
                            case ActionType.Like:
                                Token.Favorites.Create(id => x.Status.Id);
                                x.Status.IsFavorited = true;
                                break;

                            case ActionType.Quote:
                                Token.Statuses.Update(
                                    action.Message + " https://twitter.com/"
                                        + x.Status.User.ScreenName + "/status/" + x.Status.Id
                                );
                                break;

                            case ActionType.Reply:
                                Token.Statuses.Update(
                                    action.Message.Replace("%user%", x.Status.User.ScreenName),
                                    x.Status.Id
                                );
                                break;

                            case ActionType.Retweet:
                                Token.Statuses.Retweet(id => x.Status.Id);
                                x.Status.IsRetweeted = true;
                                break;

                            case ActionType.Mute: return;
                            
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }

                    if (x.Status.InReplyToUserId == _me.Id)
                    {
                        Notifications.RemoveAt(0);
                        Notifications.Add(new Tweet(x.Status));

                        _notifier.BalloonTipText =
                            x.Status.User.Name + @" (@" + x.Status.User.ScreenName
                                + ") さんからの返信\n\n" + x.Status.Text;
                        _notifier.ShowBalloonTip(3000);
                    }

                    if (x.Status.RetweetedStatus != null && x.Status.RetweetedStatus.User.Id == _me.Id)
                    {
                        _notifier.BalloonTipText =
                            x.Status.User.Name + @" (@" + x.Status.User.ScreenName
                                + ") さんがツイートをリツイートしました\n\n" + x.Status.RetweetedStatus.Text;
                        _notifier.ShowBalloonTip(3000);
                    }

                    if (x.Status.RetweetedStatus != null && x.Status.User.Id == _me.Id)
                    {
                        foreach (var t in Tweets)
                        {
                            if (t.TweetId != x.Status.RetweetedStatus.Id.ToString()) continue;
                            if (t.ColorRetweet != Conf.ColorConfig.TweetAction) break;

                            t.ColorRetweet = Conf.ColorConfig.TweetRetweet;
                            break;
                        }

                        foreach (var t in Notifications)
                        {
                            if (t.TweetId != x.Status.RetweetedStatus.Id.ToString()) continue;
                            if (t.ColorRetweet != Conf.ColorConfig.TweetAction) break;

                            t.ColorRetweet = Conf.ColorConfig.TweetRetweet;
                            break;
                        }
                    }

                    Tweets.RemoveAt(0);
                    Tweets.Add(new Tweet(x.Status));
                });

            stream.Connect();
        }

        private void OpenNewTweet(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowOrActivate<TweetWindow>();
        }

        private void OpenUserWindow(object sender, MouseButtonEventArgs e)
        {
            new UserWindow(
                long.Parse((string)((Image) sender).Tag)
            ) { Owner = this }.Show();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            WindowManager.ShowOrActivate<AutoActionsWindow>();
        }

        private async void Retweet(object sender, RoutedEventArgs e)
        {
            foreach (var t in Tweets)
            {
                if (t.TweetId != ((string)((Button)sender).Tag)) continue;
                if (t.ColorRetweet != Conf.ColorConfig.TweetAction) return;

                t.ColorRetweet = Conf.ColorConfig.TweetRetweet;
                break;
            }

            foreach (var t in Notifications)
            {
                if (t.TweetId != ((string)((Button)sender).Tag)) continue;
                if (t.ColorRetweet != Conf.ColorConfig.TweetAction) return;

                t.ColorRetweet = Conf.ColorConfig.TweetRetweet;
                break;
            }

            await Token.Statuses.RetweetAsync(id => long.Parse((string)((Button)sender).Tag));
        }

        private async void Like(object sender, RoutedEventArgs e)
        {
            foreach (var t in Tweets)
            {
                if (t.TweetId != ((string)((Button)sender).Tag)) continue;
                if (t.ColorLike != Conf.ColorConfig.TweetAction)
                {
                    t.ColorLike = Conf.ColorConfig.TweetAction;

                    try
                    {
                        await Token.Favorites.DestroyAsync(long.Parse((string)((Button)sender).Tag));
                    }
                    catch { }

                    return;
                }

                t.ColorLike = Conf.ColorConfig.TweetLike;
                break;
            }

            foreach (var t in Notifications)
            {
                if (t.TweetId != ((string)((Button)sender).Tag)) continue;
                if (t.ColorLike != Conf.ColorConfig.TweetAction)
                {
                    t.ColorLike = Conf.ColorConfig.TweetAction;

                    try
                    {
                        await Token.Favorites.DestroyAsync(long.Parse((string)((Button)sender).Tag));
                    }
                    catch {}

                    return;
                }

                t.ColorLike = Conf.ColorConfig.TweetLike;
                break;
            }

            await Token.Favorites.CreateAsync(long.Parse((string)((Button)sender).Tag));
        }

        private void QuoteTweet(object sender, RoutedEventArgs e)
        {
            new TweetWindow(MediaType.Quote, long.Parse((string)((MenuItem)sender).Tag)).Show();
        }

        private void ReplyTweet(object sender, RoutedEventArgs e)
        {
            new TweetWindow(MediaType.Reply, long.Parse((string)((MenuItem)sender).Tag)).Show();
        }

        private async void CopyTweet(object sender, RoutedEventArgs e)
        {
            Tweet status = null;

            foreach (var t in Tweets)
            {
                if (t.TweetId != ((string)((MenuItem)sender).Tag)) continue;
                status = t;
                break;
            }

            foreach (var t in Notifications)
            {
                if (t.TweetId != ((string)((MenuItem)sender).Tag)) continue;
                status = t;
                break;
            }

            if (status == null)
            {
                new MessageBox("ツイートが見つかりませんでした。", MessageBoxImage.Error);
                return;
            }
            
            await Token.Statuses.UpdateAsync(status.Text);
        }

        private async void CopyTweetWithRetweet(object sender, RoutedEventArgs e)
        {
            Tweet status = null;

            foreach (var t in Tweets)
            {
                if (t.TweetId != ((string)((MenuItem)sender).Tag)) continue;
                status = t;

                if (t.ColorRetweet != Conf.ColorConfig.TweetAction) break;
                t.ColorRetweet = Conf.ColorConfig.TweetRetweet;
                break;
            }

            foreach (var t in Notifications)
            {
                if (t.TweetId != ((string)((MenuItem)sender).Tag)) continue;
                status = t;

                if (t.ColorRetweet != Conf.ColorConfig.TweetAction) break;
                t.ColorRetweet = Conf.ColorConfig.TweetRetweet;
                break;
            }

            if (status == null)
            {
                new MessageBox("ツイートが見つかりませんでした。", MessageBoxImage.Error);
                return;
            }

            await Token.Statuses.RetweetAsync(id => long.Parse((string)((MenuItem)sender).Tag));
            await Token.Statuses.UpdateAsync(status.Text);
        }

        private void SwitchTab(object sender, RoutedEventArgs e)
        {
            if ((string)((Button)sender).Tag == "Home")
            {
                TabNotifications.BorderBrush = null;
                TabHome.BorderBrush
                    = new SolidColorBrush(StringToColor(Conf.ColorConfig.TabActive));
                NotificationsTimeline.Visibility = Visibility.Collapsed;
                Timeline.Visibility = Visibility.Visible;
            }
            else
            {
                TabHome.BorderBrush = null;
                TabNotifications.BorderBrush
                    = new SolidColorBrush(StringToColor(Conf.ColorConfig.TabActive));
                Timeline.Visibility = Visibility.Collapsed;
                NotificationsTimeline.Visibility = Visibility.Visible;
            }
        }

        private static Color StringToColor(string str)
        {
            var convertedColor = ColorConverter.ConvertFromString(str);
            if (convertedColor != null)
                return (Color)convertedColor;

            throw new ArgumentNullException();
        }

        public static MainWindow GetInstance()
        {
            return _instance;
        }

        private void DeleteNotifier(object sender, CancelEventArgs e)
        {
            _notifier.Dispose();
        }

        private async void ParseKeyboardShortcut(object sender, KeyEventArgs e)
        {
            if (e.Key == (Key)Enum.Parse(typeof(Key), Conf.KeyConfig.NewTweet))
            {
                OpenNewTweet(null, null);
                e.Handled = true;
                return;
            }

            if (e.Key == (Key)Enum.Parse(typeof(Key), Conf.KeyConfig.Home))
            {
                SwitchTab(TabHome, null);
                return;
            }

            if (e.Key == (Key)Enum.Parse(typeof(Key), Conf.KeyConfig.Notifications))
            {
                SwitchTab(TabNotifications, null);
                return;
            }

            if (e.Key == (Key)Enum.Parse(typeof(Key), Conf.KeyConfig.Like))
            {
                try
                {
                    Tweets.Last().ColorLike = Conf.ColorConfig.TweetLike;
                    await Token.Favorites.CreateAsync(long.Parse(Tweets.Last().TweetId));
                }
                catch
                {
                    return;
                }

                e.Handled = true;
                return;
            }

            if (e.Key == (Key)Enum.Parse(typeof(Key), Conf.KeyConfig.Retweet))
            {
                try
                {
                    Tweets.Last().ColorRetweet = Conf.ColorConfig.TweetRetweet;
                    await Token.Statuses.RetweetAsync(long.Parse(Tweets.Last().TweetId));
                }
                catch
                {
                    return;
                }

                e.Handled = true;
            }
        }

        private void ChangeMargin(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    LayoutRoot.Margin = new Thickness(7);
                    break;

                default:
                    LayoutRoot.Margin = new Thickness(0);
                    break;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_ENTERSIZEMOVE:
                    _isSizing = true;
                    break;
                case WM_EXITSIZEMOVE:
                    _isSizing = false;
                    PostMessage(hwnd, WM_SIZE, lastWParam, lastLParam);
                    break;
                case WM_SIZE:
                    if (_isSizing)
                    {
                        handled = true;

                        lastLParam = lParam;
                        lastWParam = wParam;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}