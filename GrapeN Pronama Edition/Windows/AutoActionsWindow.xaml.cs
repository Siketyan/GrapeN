using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace GrapeN.Windows
{
    /// <summary>
    /// AutoActionsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AutoActionsWindow : Window
    {
        public ObservableCollection<AutoAction> Actions { get; set; }

        public AutoActionsWindow()
        {
            InitializeComponent();

            Actions = MainWindow.GetInstance().Setting.AutoActions;
            Table.ItemsSource = Actions;
            DataContext = this;
        }

        void Add(object sender, RoutedEventArgs e)
        {
            if (Type.SelectedIndex == -1) { new MessageBox("アクションが設定されていません。", MessageBoxImage.Error); return; }
            if (Pattern.Text == "") { new MessageBox("空のパターンは設定できません。", MessageBoxImage.Error); return; }
            if ((Type.SelectedIndex == 2 && Message.Text == "")
             || (Type.SelectedIndex == 3 && Message.Text == ""))
            { new MessageBox("空のメッセージは設定できません。", MessageBoxImage.Error); return; }
            if (Type.SelectedIndex == -1) { new MessageBox("アクションが設定されていません。", MessageBoxImage.Error); return; }

            ActionType TypeEnum = ActionType.Like;
            if (Type.SelectedIndex == 1) TypeEnum = ActionType.Retweet;
            if (Type.SelectedIndex == 2) TypeEnum = ActionType.Quote;
            if (Type.SelectedIndex == 3) TypeEnum = ActionType.Reply;

            Actions.Add(new AutoAction(TypeEnum, (bool)UseRegex.IsChecked, Pattern.Text, Message.Text));
            MainWindow.GetInstance().Setting.AutoActions = Actions;
            SettingsWriter.Write(MainWindow.GetInstance().Setting, @".\settings.grape");
        }

        void Remove(object sender, RoutedEventArgs e)
        {
            if (Table.SelectedIndex == -1) return;

            Actions.RemoveAt(Table.SelectedIndex);
            MainWindow.GetInstance().Setting.AutoActions = Actions;
            SettingsWriter.Write(MainWindow.GetInstance().Setting, @".\settings.grape");
        }
    }
}