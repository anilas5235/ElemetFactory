using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.DataObject;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class PrefabsMono : MonoBehaviour
    {
        public GameObject itemPrefab;
        public GameObject tilePrefab;
        public GameObject tileBackgroundPrefab;

        public GameObject[] buildings;
        
        public class PrefabsBaker : Baker<PrefabsMono>
        {
            public override void Bake(PrefabsMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                //Baking Buildings--------------------------------------------------------------------------------------
                
                var buffer = AddBuffer<EntityIDPair>(entity);

                foreach (var prefab in authoring.buildings)
                {
                    var buildingAuthoring = prefab.GetComponent<BuildingMono>();
                    var buildingPrefab = GetEntity(prefab, TransformUsageFlags.Dynamic);
                    
                    buffer.Add(new EntityIDPair()
                    {
                        ID = buildingAuthoring.BuildingID,
                        Entity = buildingPrefab,
                    });
                    Debug.Log($"Crated Prefab for {buildingAuthoring.NameString}");
                }

                //Baking Items------------------------------------------------------------------------------------------
                var prefabItem = GetEntity(authoring.itemPrefab, TransformUsageFlags.Dynamic);
                
                AddComponent(entity,new ItemPrefabsDataComponent()
                {
                    Entity = entity,
                    ItemPrefab = prefabItem,
                });
                Debug.Log($"Created Prefab for Item");
                
                //Baking Tiles------------------------------------------------------------------------------------------
                
                AddComponent(entity,new TilePrefabsDataComponent()
                {
                    Entity = entity,
                    TilePrefab = GetEntity(authoring.tilePrefab,TransformUsageFlags.Dynamic),
                    TileBackGroundPrefab = GetEntity(authoring.tileBackgroundPrefab, TransformUsageFlags.Dynamic),
                });
                Debug.Log($"Created Tile Prefabs");
            }
        }
    }
}