using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CoreTweet;

namespace GrapeN.Windows
{
    /// <summary>
    /// Auth.xaml の相互作用ロジック
    /// </summary>
    public partial class AuthWindow
    {
        public Config.Config Conf { get; }

        private OAuth.OAuthSession _session;

        public AuthWindow()
        {
            InitializeComponent();
            Owner = MainWindow.GetInstance();
            Conf = MainWindow.GetInstance().Conf;
        }

        private void CheckPin(object sender, TextCompositionEventArgs e)
        {
            bool yesParse;
            {
                {
                    float xx;
                    var tmp = Pin.Text + e.Text;
                    yesParse = float.TryParse(tmp, out xx);
                }
            }

            e.Handled = !yesParse;
        }

        private void AuthStart(object sender, RoutedEventArgs e)
        {
            _session = OAuth.Authorize(
                "eA8yVmShCT0Y494DSDOW9PUgt",
                "h1o2qlALbloAkeFckIghl7ajB0fuqi3n8LE7y1xVJ5pMod0XYv"
            );

            Process.Start(_session.AuthorizeUri.ToString());
        }

        private void GetToken(object sender, RoutedEventArgs e)
        {
            var token = _session.GetTokens(Pin.Text);
            var setting = new Settings
            {
                AccessToken = token.AccessToken,
                AccessTokenSecret = token.AccessTokenSecret,
                AutoActions = new ObservableCollection<AutoAction>()
            };

            SettingsWriter.Write(setting, @".\settings.grape");
            Close();
        }
    }
}