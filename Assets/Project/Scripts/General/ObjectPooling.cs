using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Scripts.General
{
    /// <summary>
    ///   <para>All the deriving classes should act as an ObjectPool for a single type of GameObject that should be referenced by the "objectToPool" variable</para>   
    /// </summary>
    public abstract class ObjectPooling<T> : Singleton<T> where T:MonoBehaviour
    {
        [SerializeField] protected GameObject objectToPool;
        private List<GameObject> objectPool;
        private int totalObjectCount;
        
        protected virtual void Start()
        {
            objectPool = new List<GameObject>() ;
        }

        /// <summary>
        ///   <para>This functions adds an existing GameObject that is not part of the Pool to the Pool</para>   
        /// </summary>
        
        public virtual void AddObjectToPool(GameObject obj)
        {
            if (objectPool.Contains(obj)) return;
            AddObjectExtraCommands(obj);
            objectPool.Add(obj);
            UpdateName();
            obj.SetActive(false);
        }

        protected virtual void AddObjectExtraCommands(GameObject obj)
        {
            obj.transform.position = transform.position;
            obj.transform.SetParent(transform);
        }
        
        /// <summary>
        ///   <para>This functions gives back an GameObject that was extracted from the Pool and can now be used</para>   
        /// </summary>

        public virtual GameObject GetObjectFromPool()
        {
            objectPool ??= new List<GameObject>();
            GameObject returnObj;
            if (!objectPool.Any())
            {
                returnObj = CreateNewPoolObject();
                returnObj.gameObject.name = $"{objectToPool.name}({totalObjectCount})";
                totalObjectCount++;
            }
            else
            {
                returnObj = objectPool.First();
                objectPool.Remove(returnObj);
                GetObjectExtraCommands(returnObj);
                returnObj.SetActive(true);
            }
            
            UpdateName();
            return returnObj;
        }

        protected virtual GameObject CreateNewPoolObject()
        {
            return Instantiate(objectToPool, transform.position, Quaternion.identity);
        }
        
        protected virtual void GetObjectExtraCommands(GameObject obj)
        {
            obj.transform.SetParent(null);
        }

        private void UpdateName()
        {
            gameObject.name = $"{objectToPool.name}Pool({transform.childCount})";
        }
    }
}
