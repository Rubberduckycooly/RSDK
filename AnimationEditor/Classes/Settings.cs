using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace AnimationEditor
{
    public static class Settings
    {
        public static string GetAppDataPath()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\RSDK Animation Editor")) Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\RSDK Animation Editor");
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\RSDK Animation Editor";
        }

        private static string FilePath { get => GetAppDataPath() + "\\appConfig.json"; }

        public class Instance
        {
            public List<RecentFile> RecentFiles { get; set; } = new List<RecentFile>();

            public bool exportFullJson = false;

            public class RecentFile
            {
                public string FilePath { get; set; }
                public int Format { get; set; }

                public RecentFile(string filePath = "", int format = 5)
                {
                    FilePath = filePath;
                    Format = format;
                }
            }
        }

        public static Instance Default { get; set; }

        public static void Init()
        {
            Reload();
        }
        public static void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Default);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static void Reload()
        {
            try
            {
                if (!File.Exists(FilePath)) File.Create(FilePath).Close();
                string json = File.ReadAllText(FilePath);
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    Instance result = JsonConvert.DeserializeObject<Instance>(json, settings);
                    if (result != null) Default = result;
                    else Default = new Instance();
                }
                catch
                {
                    Default = new Instance();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Default = new Instance();
            }

        }
        public static void Reset()
        {
            Default = new Instance();
            Save();
            Reload();
        }
    }
}
