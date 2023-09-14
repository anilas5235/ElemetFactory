using System;
using Project.Scripts.Utilities;

namespace UI
{
    public static class SaveSettings
    {
        private const string PathString = "Settings.txt";

        private static SettingsSaveData currentSaveSettings;

        public static SettingsSaveData CurrentSaveSettings
        {
            get
            {
                currentSaveSettings ??= SaveSystem.Load<SettingsSaveData>(PathString);
                return currentSaveSettings;
            }
            set => currentSaveSettings = value;
        }

        public static void Save()
        {
            SaveSystem.Save(PathString, currentSaveSettings);
        }
    }
}

   