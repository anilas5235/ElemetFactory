using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class BuildingPrefabsMono : MonoBehaviour
    {
        public BuildingScriptableData[] buildingScriptableData;

        public class BuildingPrefabsBaker : Baker<BuildingPrefabsMono>
        {
            public override void Bake(BuildingPrefabsMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var buffer = AddBuffer<EntityIDPair>(entity);

                foreach (var scriptableData in authoring.buildingScriptableData)
                {
                    buffer.Add(new EntityIDPair()
                    {
                        ID = scriptableData.buildingID,
                        Entity = GetEntity(scriptableData.prefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
}