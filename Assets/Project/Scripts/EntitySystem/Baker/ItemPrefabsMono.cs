using Project.Scripts.EntitySystem.Components.DataObject;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class ItemPrefabsMono : MonoBehaviour
    {
        public GameObject itemPrefab;
        private class ItemPrefabsMonoBaker : Baker<ItemPrefabsMono>
        {
            public override void Bake(ItemPrefabsMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity,new ItemPrefabsDataComponent()
                {
                    ItemPrefab = GetEntity(authoring.itemPrefab,TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}