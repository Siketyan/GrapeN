using CoreTweet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static CoreTweet.OAuth;

namespace GrapeN
{
    /// <summary>
    /// Auth.xaml の相互作用ロジック
    /// </summary>
    public partial class Auth : Window
    {
        OAuthSession Session;

        public Auth()
        {
            InitializeComponent();
            Owner = MainWindow.GetInstance();
        }

        void CheckPin(object sender, TextCompositionEventArgs e)
        {
            bool yes_parse = false;
            {
                // 既存のテキストボックス文字列に、
                // 今新規に一文字追加された時、その文字列が
                // 数値として意味があるかどうかをチェック
                {
                    float xx;
                    var tmp = Pin.Text + e.Text;
                    yes_parse = float.TryParse(tmp, out xx);
                }
            }

            e.Handled = !yes_parse;
        }

        void AuthStart(object sender, RoutedEventArgs e)
        {
            Session = Authorize(
                "eA8yVmShCT0Y494DSDOW9PUgt",
                "h1o2qlALbloAkeFckIghl7ajB0fuqi3n8LE7y1xVJ5pMod0XYv"
            );

            Process.Start(Session.AuthorizeUri.ToString());
        }

        void GetToken(object sender, RoutedEventArgs e)
        {
            Tokens Token = OAuth.GetTokens(Session, Pin.Text);

            Settings Setting = new Settings();
            Setting.AccessToken = Token.AccessToken;
            Setting.AccessTokenSecret = Token.AccessTokenSecret;
            SettingsWriter.Write(Setting, @".\settings.grape");

            Close();
        }
    }
}
