using Ionic.Zip;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrapeN_Updater
{
    public partial class Progress : Form
    {
        WebClient wc;
        string latest;
        bool isSilent;
        
        public Progress(string latest, bool isSilent)
        {
            this.latest = latest;
            this.isSilent = isSilent;

            InitializeComponent();
        }

        void Init(object sender, EventArgs e)
        {
            if (!isSilent) WindowState = FormWindowState.Normal;

            Uri Url = new Uri("https://dl.sikeserver.com/pub/GrapeN-Updater/grapen-update-" + latest + ".zip");
            wc = new WebClient();

            wc.DownloadProgressChanged +=
                    new DownloadProgressChangedEventHandler(OnDownloadProgressChange);
            wc.DownloadFileCompleted +=
                    new AsyncCompletedEventHandler(OnDownloadCompleted);

            ProgressBar.Style = ProgressBarStyle.Continuous;
            wc.DownloadFileAsync(Url,
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp\grapen-" + latest + ".zip"
            );
        }

        async void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ProgressBar.Style = ProgressBarStyle.Marquee;
            await Task.Run(() =>
            {
                using (
                    ZipFile zip = ZipFile.Read(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                            + @"\Temp\grapen-" + latest + ".zip"
                    )
                )
                {
                    zip.ExtractAll(
                        Environment.CurrentDirectory, ExtractExistingFileAction.OverwriteSilently
                    );
                }
            });
            MessageBox.Show("GrapeNを最新バージョン " + latest + " をインストールしました。", "GrapeN Updater",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        void OnDownloadProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadedSize.Text = e.BytesReceived + "B / " + e.TotalBytesToReceive + "B";
            ProgressBar.Value = e.ProgressPercentage;
        }
    }
}
