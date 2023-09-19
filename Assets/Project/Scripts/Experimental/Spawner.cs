using System.Collections;
using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class Spawner : MonoBehaviour
    {
        public GameObject objectToSpawn;
        public int amount= 100;
        public int batchSize = 30;
        public int CubeSize = 11;

        public int totalSpawned;

        public bool makeCube = false;

        // Start is called before the first frame update
        private void Start()
        {
            totalSpawned = 0;
            StartCoroutine(makeCube ? SpawnCube(CubeSize) : Spawn(amount));
        }

        private IEnumerator SpawnCube(int size)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        GameObject gameObject = Instantiate(objectToSpawn,
                            new Vector3(i * objectToSpawn.transform.localScale.x,
                                j * objectToSpawn.transform.localScale.y, k * objectToSpawn.transform.localScale.z),
                            Quaternion.identity);
                    }
                }
                yield return null;
            }
        }


        private IEnumerator Spawn(int number)
        {
            int batchNumber = 0;
            while (totalSpawned < number)
            {
                int thisBatchSize = batchSize;
                thisBatchSize = Mathf.Clamp(thisBatchSize, 0, number - totalSpawned);

                for (int i = 0; i < thisBatchSize; i++)
                {
                    GameObject gameObject = Instantiate(objectToSpawn,
                        new Vector3(batchNumber * objectToSpawn.transform.localScale.x,
                            i * objectToSpawn.transform.localScale.y, 0), Quaternion.identity);
                }

                batchNumber++;
                totalSpawned += thisBatchSize;
                yield return null;
            }
        }
    }
}
