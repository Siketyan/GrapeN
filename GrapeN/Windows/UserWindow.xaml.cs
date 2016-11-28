using CoreTweet;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GrapeN.Windows
{
    /// <summary>
    /// UserWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UserWindow : Window
    {
        private readonly Tokens _token;
        private readonly long _userId;
        private readonly string _userName;

        private User _user;
        
        public UserWindow(long userId)
        {
            InitializeComponent();
            _token = MainWindow.GetInstance().Token;
            _userId = userId;
        }

        public UserWindow(string userName)
        {
            InitializeComponent();
            _token = MainWindow.GetInstance().Token;
            _userName = userName;
        }

        private async void Init(object sender, RoutedEventArgs e)
        {
            _user = (_userName == null) ? await _token.Users.ShowAsync(_userId)
                                   : await _token.Users.ShowAsync(_userName);

            Banner.Source = new BitmapImage(new Uri(_user.ProfileBannerUrl));
            ProfileIcon.Source = new BitmapImage(new Uri(_user.ProfileImageUrlHttps));

            Title = _user.Name + " (@" + _user.ScreenName + ")";
            ProfileName.Text = _user.Name;
            ScreenName.Text = "@" + _user.ScreenName;
            Description.Text = _user.Description;

            Tweets.Text = $"{_user.StatusesCount:#,0}";
            Follows.Text = $"{_user.FriendsCount:#,0}";
            Followers.Text = $"{_user.FollowersCount:#,0}";
            Lists.Text = $"{_user.ListedCount:#,0}";

            IsVerified.Visibility = (_user.IsVerified) ? Visibility.Visible : Visibility.Collapsed;
            IsLocked.Visibility = (_user.IsProtected) ? Visibility.Visible : Visibility.Collapsed;

            UpdateFriendship();
        }

        private async void UpdateFriendship()
        {
            IList<string> con = (await _token.Friendships.LookupAsync(new List<long> { (long)_user.Id }))[0].Connections;

            if (con.Contains("following"))
            {
                var convertedColor = ColorConverter.ConvertFromString("#3399FF");
                if (convertedColor != null)
                    IsFollowed.Foreground = new SolidColorBrush((Color)convertedColor);

                IsFollowed.ToolTip = "フォローしています";
            }

            if (con.Contains("followed_by"))
            {
                IsFollowed.Foreground = Brushes.White;
                IsFollowed.ToolTip = "フォローされています";
            }

            if (!con.Contains("following") || !con.Contains("followed_by")) return;
            IsFollowed.Foreground = Brushes.LightGreen;
            IsFollowed.ToolTip = "相互フォローしています";
        }

        /*
        async void FollowUser(object sender, RoutedEventArgs e)
        {
            if ((string)IsFollowed.ToolTip == "相互フォローしています"
             || (string)IsFollowed.ToolTip == "フォローしています") // フォロー解除
            {
                new MessageBox("hoge", MessageBoxImage.Information);
                await _token.Friendships.DestroyAsync(user_id => _user.Id);
                UpdateFriendship();
            }
            else // フォローする
            {
                await _token.Friendships.CreateAsync(user_id => _user.Id);
                UpdateFriendship();
            }
        }
        */
    }
}
