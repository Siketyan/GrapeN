using System.Runtime.ExceptionServices;
using System.Windows;

namespace GrapeN.Windows
{
    /// <summary>
    /// ExceptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow(FirstChanceExceptionEventArgs e)
        {
            InitializeComponent();

            Message.Text =
                "Target: " + e.Exception.TargetSite.Name
                    + "\nError: " + e.Exception.Message
                    + "\n====== StackTrace ======\n"
                    + e.Exception.StackTrace;
        }
    }
}