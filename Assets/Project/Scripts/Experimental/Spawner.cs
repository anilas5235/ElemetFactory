using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class Spawner : MonoBehaviour
    {
        public GameObject objectToSpawn;
        public int amount= 100;
        public int batchSize = 30;

        public int totalSpawned;
        // Start is called before the first frame update
        void Start()
        {
            totalSpawned = 0;
            StartCoroutine(Spawn(amount));
        }


        IEnumerator Spawn(int number)
        {
            int batchNumber = 0;
            while (totalSpawned < number)
            {
                int thisBatchSize = batchSize;
                thisBatchSize = Mathf.Clamp(thisBatchSize, 0, number - totalSpawned);
                
                for (int i = 0; i < thisBatchSize; i++)
                {
                    for (int j = 0; j < thisBatchSize; j++)
                    {

                        GameObject gameObject = Instantiate(objectToSpawn, new Vector3(batchNumber * objectToSpawn.transform.localScale.x, i * objectToSpawn.transform.localScale.y, j* objectToSpawn.transform.localScale.z), Quaternion.identity);
                        gameObject.GetComponentInChildren<Renderer>()?.material.SetColor("_ContentColor", new Color(batchNumber/10f,i/10f,j/10f,1f));
                        
                        const string basePath = "Assets/Project/Resources/Materials/Gas_Bottle/";
                        string fileName =batchNumber.ToString("00") + i.ToString("00") + j.ToString("00");
                        string path = basePath+$"{fileName}.asset";

                        if (!System.IO.File.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), path)))
                        {

                            Material material = new Material(Shader.Find("GasBottleShader"));
                            material.SetColor("_ContentColor", new Color(batchNumber / 10f, i / 10f, j / 10f, 1f));
                            material.name = fileName;

                            AssetDatabase.CreateAsset(material,"Materials/Gas_Bottle/");
                        }
                    }
                }
                
                AssetDatabase.SaveAssets();

                batchNumber++;
                totalSpawned += thisBatchSize;
                yield return null;
            }
        }
    }
}
