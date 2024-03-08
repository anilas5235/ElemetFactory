using Project.Scripts.EntitySystem.Components.DataObject;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class TilePrefabsMono : MonoBehaviour
    {
        public GameObject tilePrefab;
        public GameObject tileBackGroundPrefab;
        private class TilePrefabsMonoBaker : Baker<TilePrefabsMono>
        {
            public override void Bake(TilePrefabsMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity,new TilePrefabsDataComponent()
                {
                    Entity = entity,
                    TilePrefab = GetEntity(authoring.tilePrefab,TransformUsageFlags.Dynamic),
                    TileBackGroundPrefab = GetEntity(authoring.tileBackGroundPrefab,TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}