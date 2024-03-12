using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Tags;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.EntitySystem.Baker
{
    public class PrefabsMono : MonoBehaviour
    {
        public GameObject 
            ItemPrefab,
            TilePrefab,
            TileBackgroundPrefab;

        public BuildingScriptableData[] BuildingScriptableData;
        private static ItemScriptableData[] GetItemScriptableDataData()
        {
            return Resources.LoadAll<ItemScriptableData>("Data/Items");
        }
        
        public class PrefabsBaker : Baker<PrefabsMono>
        {
            public override void Bake(PrefabsMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                //Baking Buildings--------------------------------------------------------------------------------------
                
                var buffer = AddBuffer<EntityIDPair>(entity);

                foreach (var scriptableData in authoring.BuildingScriptableData)
                {
                    var buildingPrefab = GetEntity(scriptableData.prefab, TransformUsageFlags.Dynamic);

                    var inBuff = SetBuffer<InputSlot>(entity);
                    for (var i = 0; i < scriptableData.InputOffsets.Length; i++)
                    {
                        inBuff.Add(new InputSlot());
                    }

                    var outBuff = SetBuffer<OutputSlot>(entity);
                    for (var i = 0; i < scriptableData.OutputOffsets.Length; i++)
                    {
                        outBuff.Add(new OutputSlot());
                    }

                    buffer.Add(new EntityIDPair()
                    {
                        ID = scriptableData.buildingID,
                        Entity = buildingPrefab,
                    });
                    Debug.Log($"Crated Prefab for {scriptableData.nameString}");
                }

                //Baking Items------------------------------------------------------------------------------------------
                var prefabItem = GetEntity(authoring.ItemPrefab, TransformUsageFlags.Dynamic);
                
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
                    TilePrefab = GetEntity(authoring.TilePrefab,TransformUsageFlags.Dynamic),
                    TileBackGroundPrefab = GetEntity(authoring.TileBackgroundPrefab, TransformUsageFlags.Dynamic),
                });
                Debug.Log($"Created Tile Prefabs");
            }
        }
    }
}