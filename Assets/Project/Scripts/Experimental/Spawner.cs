using System.Collections;
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
                    Instantiate(objectToSpawn, new Vector3(batchNumber*objectToSpawn.transform.localScale.x, i*objectToSpawn.transform.localScale.y, 0), Quaternion.identity);
                }

                batchNumber++;
                totalSpawned += thisBatchSize;
                yield return null;
            }
        }
    }
}
