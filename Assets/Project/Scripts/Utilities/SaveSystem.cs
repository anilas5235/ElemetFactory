using System.IO;
using System.Xml.Serialization;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class SaveSystem
    {
        public static void Save<T>(string path, T saveData) where T : class, new()
        {
            path = Application.persistentDataPath + "/" + path;

            var serializer = new XmlSerializer(typeof(T));
            var stream = new FileStream(path, FileMode.Create);
            serializer.Serialize(stream, saveData);
            stream.Close();

            Debug.Log("Save was successful");
        }

        public static T Load<T>(string path) where T : class, new()
        {
            path = Application.persistentDataPath + "/" + path;
            Debug.Log(path);

            T activeSave;
            
            if (File.Exists(path))
            {
                var serializer = new XmlSerializer(typeof(T));
                var stream = new FileStream(path, FileMode.Open);
                activeSave = serializer.Deserialize(stream) as T;
                stream.Close();

                Debug.Log("Load was successful");
            }
            else
            {
                activeSave = new T();
            }
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
        public float[] audioOptions = new float[10];
        public bool[] levelsUnlocked = new bool[10];
        public ChunkSave[] chunkSaves;
    }
}