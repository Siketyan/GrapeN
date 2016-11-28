using GrapeN.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace GrapeN
{
    public class HyperLink : TextBlock
    {
        private static Encoding m_Enc = Encoding.GetEncoding("Shift_JIS");

        public static readonly DependencyProperty ArticleContentProperty =
            DependencyProperty.RegisterAttached(
                "Inline",
                typeof(string),
                typeof(HyperLink),
                new PropertyMetadata(null, OnInlinePropertyChanged));

        public static string GetInline(TextBlock element)
        {
            return (element != null) ? element.GetValue(ArticleContentProperty) as string : string.Empty;
        }

        public static void SetInline(TextBlock element, string value)
        {
            if (element != null)
                element.SetValue(ArticleContentProperty, value);
        }

        private static void OnInlinePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var tb = obj as TextBlock;
            var msg = e.NewValue as string;
            if (tb == null || msg == null)
                return;

            // 末尾の改行コードを取り除く
            msg = msg.TrimEnd(new char[] { '\n', '\r' });

            // 改行位置の取得
            var nl = new List<int>();
            int i = 0;
            while ((i = msg.IndexOf("\r\n", i)) >= 0)
            {
                nl.Add(i - (nl.Count * 2));
                i += 2;
            }
            nl.Sort();

            // 正規表現でURLアドレスを検出
            var regex = new Regex(@"(http(s)?://([\w-]+\.)+[\w-]+(/[A-Z0-9-.,_/?%&=]*)?|@[0-9a-zA-Z_]+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var text = msg.Replace("\r\n", "");
            var matchs = regex.Matches(text);
            if (matchs.Count <= 0)
            {
                tb.Text = msg;
                return;
            }
            tb.Text = null;
            tb.Inlines.Clear();

            int pos = 0;
            int l = 0;
            foreach (Match match in matchs)
            {
                var index = match.Groups[0].Index;
                var length = match.Groups[0].Length;
                var tag = match.Groups[0].Value;

                // 文章前部の非リンク文字列を挿入
                if (pos < index)
                {
                    while (pos < text.Length)
                    {
                        if (nl.Count - l > 0 && nl[l] < index)
                        {
                            var buff = text.Substring(pos, nl[l] - pos);
                            tb.Inlines.Add(new Run(buff));
                            tb.Inlines.Add(new LineBreak());
                            pos = nl[l];
                            l++;
                        }
                        else
                        {
                            var buff = text.Substring(pos, index - pos);
                            tb.Inlines.Add(new Run(buff));
                            pos = index;
                            break;
                        }
                    }
                }

                // リンクの作成
                var link = new Hyperlink();
                link.TextDecorations = null;
                link.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)51, (byte)153, (byte)255));
                link.Tag = tag;
                link.RequestNavigate += new RequestNavigateEventHandler(RequestNavigate);
                link.Click += new RoutedEventHandler(LinkClicked);

                while (pos < text.Length)
                {
                    if (nl.Count - l > 0 && nl[l] < index + length)
                    {
                        var buff = text.Substring(pos, nl[l] - pos);
                        link.Inlines.Add(new Run(buff));
                        link.Inlines.Add(new LineBreak());
                        pos = nl[l];
                        l++;
                    }
                    else
                    {
                        var buff = text.Substring(pos, index + length - pos);
                        link.Inlines.Add(new Run(buff));
                        pos = index + length;
                        break;
                    }
                }

                // Hyperlinkの追加
                tb.Inlines.Add(link);
            }

            // 文章後部の非リンク文字列を挿入
            while (pos < text.Length)
            {
                if (nl.Count - l > 0)
                {
                    var buff = text.Substring(pos, nl[l] - pos);
                    tb.Inlines.Add(new Run(buff));
                    tb.Inlines.Add(new LineBreak());
                    pos = nl[l];
                    l++;
                }
                else
                {
                    var buff = text.Substring(pos, text.Length - pos);
                    tb.Inlines.Add(new Run(buff));
                    pos = text.Length;
                    break;
                }
            }
        }

        private static void RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            return;
        }

        private static void LinkClicked(object sender, RoutedEventArgs e)
        {
            string Uri = (string)((Hyperlink)sender).Tag;
            if (Uri.Substring(0, 1) == "@")
            {
                UserWindow w = new UserWindow(Uri.Substring(1));
                w.Owner = MainWindow.GetInstance();
                w.Show();
            }
            else
            {
                Process.Start(Uri);
            }
        }
    }
}
