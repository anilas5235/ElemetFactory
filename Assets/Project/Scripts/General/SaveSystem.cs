using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Project.Scripts.General
{
    /*
     * Based on code form Michael Lamberz
     */
    public class SaveSystem : Singleton<SaveSystem>
    {
        private SaveData activeSave;

        protected override void Awake()
        {
            base.Awake();
            if(activeSave == null) Load();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public void Save()
        {
            string path = Application.persistentDataPath + "/" + "PlayerSave.hotf";

            var serializer = new XmlSerializer(typeof(SaveData));
            var stream = new FileStream(path, FileMode.Create);
            serializer.Serialize(stream, activeSave);
            stream.Close();

            Debug.Log("Save was successful");
        }

        public void Load()
        {
            string path = Application.persistentDataPath + "/" + "PlayerSave.hotf";
            Debug.Log(path);

            if (File.Exists(path))
            {
                var serializer = new XmlSerializer(typeof(SaveData));
                var stream = new FileStream(path, FileMode.Open);
                activeSave = serializer.Deserialize(stream) as SaveData;
                stream.Close();

                Debug.Log("Load was successful");
            }
            else
            {
                //create new SaveData with default values
                activeSave = new SaveData();

                for (int i = 0; i < activeSave.audioOptions.Length; i++) activeSave.audioOptions[i] = -2f;
                activeSave.levelsUnlocked[0] = true;
            }
        }

        public void DeleteSaveData()
        {
            string path = Application.persistentDataPath + "/" + "PlayerSave.save";

            if (File.Exists(path)) File.Delete(path);
        }

        public SaveData GetActiveSave() { return activeSave; }
    }

    [System.Serializable]
    public class SaveData
    {
        public float[] audioOptions = new float[10];
        public bool[] levelsUnlocked = new bool[10];
    }
}