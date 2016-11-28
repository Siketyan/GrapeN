using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GrapeN_Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            string latest = wc.DownloadString("https://dl.sikeserver.com/pub/GrapeN-Updater/VERSION");
            string current = "";

            if (File.Exists(@".\version.grape")) current = File.ReadAllText(@".\version.grape");
            else
            {
                StreamWriter Writer = File.CreateText(@".\version.grape");
                Writer.Write(latest);
                Writer.Flush();
                Writer.Close();
            }

            if (latest == current)
            {
                Process.Start(@".\GrapeN.exe");
                return;
            }
            
            Progress ProgressWindow = (args.Length != 0 && args[0] == "-s") ? new Progress(latest, true) : new Progress(latest, false);
            ProgressWindow.ShowDialog();
            Process.Start(@".\GrapeN.exe");
        }
    }
}
