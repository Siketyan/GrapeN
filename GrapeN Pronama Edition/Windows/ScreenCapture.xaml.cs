using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;

namespace GrapeN
{
    /// <summary>
    /// ScreenCapture.xaml の相互作用ロジック
    /// </summary>
    public partial class ScreenCapture : Window
    {
        public bool Result { get; private set; }
        public string FileName { get; private set; }

        public ScreenCapture()
        {
            InitializeComponent();

            MouseLeftButtonDown += (sender, e) => DragMove();
        }

        void Confirm(object sender, RoutedEventArgs e)
        {
            UpdateLayout();
            
            Border b = CaptureRange;
            Bitmap Bmp = new Bitmap((int)b.ActualWidth, (int)b.ActualHeight);
            Graphics Graphic = Graphics.FromImage(Bmp);

            System.Windows.Point WSrc = b.PointToScreen(new System.Windows.Point(0, 0));

            System.Drawing.Point Src = new System.Drawing.Point((int)WSrc.X, (int)WSrc.Y);
            System.Drawing.Point Dest = new System.Drawing.Point(0, 0);

            Graphic.CopyFromScreen(
                Src, Dest,
                new System.Drawing.Size((int)b.ActualWidth, (int)b.ActualHeight)
            );

            FileName = "~$Capture-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
            Bmp.Save(FileName, ImageFormat.Png);
            Result = true;
            Close();
        }

        void Cancel(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
