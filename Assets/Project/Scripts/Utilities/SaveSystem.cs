using System.IO;
using Project.Scripts.Grid.DataContainers;
using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class SaveSystem
    {
        public static void Save<T>(string path, T saveData) where T : class, new()
        {
            path = Application.persistentDataPath + "/" + path;
            
            File.WriteAllText(path, JsonUtility.ToJson(saveData,true));
        }

        public static T Load<T>(string path) where T : class, new()
        {
            path = Application.persistentDataPath + "/" + path;
            Debug.Log(path);

            var activeSave = File.Exists(path) ? JsonUtility.FromJson<T>(File.ReadAllText(path)) : new T();
            return activeSave;
        }

        public static void DeleteSaveData(string path)
        {
            path = Application.persistentDataPath + "/" + path;

            if (File.Exists(path)) File.Delete(path);
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public ChunkSave[] chunkSaves;
    }
}