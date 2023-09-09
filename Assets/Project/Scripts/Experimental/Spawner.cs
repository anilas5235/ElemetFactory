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

        public Shader Shader;

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
                    for (int j = 0; j < 1; j++)
                    {

                        GameObject gameObject = Instantiate(objectToSpawn, new Vector3(batchNumber * objectToSpawn.transform.localScale.x, i * objectToSpawn.transform.localScale.y, j* objectToSpawn.transform.localScale.z), Quaternion.identity);
                        Renderer _renderer = gameObject.GetComponentInChildren<Renderer>();
                        Vector3 color = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))*10;
                        Material[] materials = _renderer.materials;
                        string path = $"Materials/Gas_Bottle/{color.x:00}{color.y:00}{color.z:00}";
                        materials[0] = Resources.Load<Material>(path);
                        _renderer.materials = materials;

                    }
                }

                batchNumber++;
                totalSpawned += thisBatchSize;
                yield return null;
            }
        }
    }
}
