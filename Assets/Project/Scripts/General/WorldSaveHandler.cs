using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Utilities;

namespace Project.Scripts.General
{
    public static class WorldSaveHandler
    {
        private static string pathString = "World.save";
        public static WorldSave CurrentWorldSave { get; private set; }

        public static WorldSave GetWorldSave()
        {
            CurrentWorldSave ??= SaveSystem.Load<WorldSave>(pathString);
            return CurrentWorldSave;
        }

        public static void SaveWorldToFile()
        {
            SaveSystem.Save(pathString,CurrentWorldSave);
        }
    }
}
