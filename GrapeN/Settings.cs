using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GrapeN.Windows;

namespace GrapeN
{
    [Serializable]
    public class Settings
    {
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public ObservableCollection<AutoAction> AutoActions { get; set; }
    }

    public class SettingsLoader : IDisposable
    {
        public string Path { get; private set; }
        public Settings Setting { get; }
        private FileStream Stream { get; }

        public SettingsLoader(string path)
        {
            Path = path;

            if (!File.Exists(path))
            {
                new AuthWindow().ShowDialog();
            }

            Stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            Setting = (Settings) new BinaryFormatter().Deserialize(Stream);
        }

        public void Save()
        {
            new BinaryFormatter().Serialize(Stream, Setting);
        }

        public void Dispose()
        {
            Stream.Close();
            Stream.Dispose();
        }
    }

    public static class SettingsWriter
    {
        public static void Write(Settings setting, string path)
        {
            if (!File.Exists(@".\settings.grape"))
                File.Create(@".\settings.grape").Close();

            var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            new BinaryFormatter().Serialize(stream, setting);

            stream.Flush();
            stream.Close();
            stream.Dispose();
        }
    }
}