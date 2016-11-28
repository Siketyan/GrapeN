using System;
using System.Collections.Generic;
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

namespace GrapeN
{
    /// <summary>
    /// MessageBox.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageBox : Window
    {
        public string IconImage { get; set; }

        public MessageBox(string Message, MessageBoxImage Icon)
        {
            InitializeComponent();
            DataContext = this;
            
            switch (Icon)
            {
                case MessageBoxImage.Error:
                    IconImage = "../Resources/error.png";
                    break;

                case MessageBoxImage.Information:
                    IconImage = "../Resources/info.png";
                    break;

                case MessageBoxImage.Question:
                    IconImage = "../Resources/question.png";
                    break;

                case MessageBoxImage.Warning:
                    IconImage = "../Resources/warning.png";
                    break;
                
                default:
                    IconImage = "../Resources/info.png";
                    break;
            }

            this.Message.Text = Message;
            Owner = MainWindow.GetInstance();
            ShowDialog();
        }

        void WindowClose(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
